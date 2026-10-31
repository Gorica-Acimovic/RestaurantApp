import { Component, OnDestroy, OnInit } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { FormBuilder, FormControl, FormGroup, FormGroupDirective, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { merge, Subscription } from 'rxjs';
import { ToastrService } from 'ngx-toastr';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatTableModule } from '@angular/material/table';
import { OrderService } from '../../services/order.service';
import { TableService } from '../../services/table.service';
import { MenuService } from '../../services/menu.service';
import { RealtimeService } from '../../services/realtime.service';
import { InvoiceDto, OrderDto, OrderItemDto, PaymentMethod } from '../../models/order.models';
import { MenuGroup } from '../../models/menu.models';
import { TableDto } from '../../models/table.models';
import { groupMenuItems } from '../../menu-groups';
import { CURRENCY } from '../../currency';

@Component({
  selector: 'app-order',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    DatePipe,
    DecimalPipe,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatTableModule
  ],
  templateUrl: './order.component.html',
  styleUrl: './order.component.css'
})
export class OrderComponent implements OnInit, OnDestroy {
  tableId = 0;
  table: TableDto | null = null;
  order: OrderDto | null = null;
  invoice: InvoiceDto | null = null;
  menuGroups: MenuGroup[] = [];
  loaded = false;
  errorMessage = '';
  openError = '';
  sending = false;
  form: FormGroup;
  addForm: FormGroup;
  paymentControl = new FormControl<PaymentMethod>('Cash', { nonNullable: true });
  displayedColumns = ['name', 'quantity', 'unitPrice', 'lineTotal', 'note', 'actions'];
  readonly currency = CURRENCY;

  private refreshSubscription: Subscription | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private orderService: OrderService,
    private tableService: TableService,
    private menuService: MenuService,
    private realtimeService: RealtimeService,
    private formBuilder: FormBuilder,
    private toastr: ToastrService
  ) {
    this.form = this.formBuilder.group({
      numberOfGuests: [2, [Validators.required, Validators.min(1), Validators.max(50)]]
    });

    this.addForm = this.formBuilder.group({
      menuItemId: [null, [Validators.required]],
      quantity: [1, [Validators.required, Validators.min(1), Validators.max(99)]],
      note: ['', [Validators.maxLength(200)]]
    });
  }

  ngOnInit(): void {
    this.tableId = Number(this.route.snapshot.paramMap.get('tableId'));

   this.tableService.getAllTables().subscribe({
  next: (data) => {
    this.table = data.find(t => t.id === this.tableId) ?? null;

    if (this.table) {
      const control = this.form.controls['numberOfGuests'];
      control.addValidators(Validators.max(this.table.capacity));
      control.setValue(Math.min(control.value, this.table.capacity));
    }
  },
  error: () => (this.errorMessage = 'Could not load the table.')
});

    this.menuService.getMenuItems('', '').subscribe({
      next: (data) => (this.menuGroups = groupMenuItems(data)),
      error: () => (this.errorMessage = 'Could not load the menu.')
    });

    this.loadOrder();
this.realtimeService.joinTable(this.tableId);
   this.refreshSubscription = merge(
  this.realtimeService.tablesChanged$,
  this.realtimeService.orderChanged$,
  this.realtimeService.reconnected$
).subscribe(() => this.loadOrder());
  }

  ngOnDestroy(): void {
  this.refreshSubscription?.unsubscribe();
  this.realtimeService.leaveTable(this.tableId);
}

  loadOrder(): void {
    this.orderService.getOpenOrderForTable(this.tableId).subscribe({
      next: (data) => {
        this.order = data;
        this.loaded = true;
      },
      error: (err) => {
        if (err.status === 404) {
          this.order = null;
          this.loaded = true;
        } else {
          this.errorMessage = 'Could not load the order.';
        }
      }
    });
  }

  openOrder(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.openError = '';
    this.sending = true;

    this.orderService
      .openOrder({ tableId: this.tableId, numberOfGuests: this.form.getRawValue().numberOfGuests })
      .subscribe({
        next: (data) => {
          this.order = data;
          this.sending = false;
          this.toastr.success('Order opened.');
        },
        error: (err) => {
          this.openError = err.error?.message ?? 'Something went wrong. Please try again.';
          this.sending = false;

          if (err.status === 409) {
            this.loadOrder();
          }
        }
      });
  }

  addItem(directive: FormGroupDirective): void {
    if (!this.order) {
      return;
    }

    if (this.addForm.invalid) {
      this.addForm.markAllAsTouched();
      return;
    }

    const { menuItemId, quantity, note } = this.addForm.getRawValue();

    this.orderService
      .addItem(this.order.id, { menuItemId, quantity, note: (note ?? '').trim() || null })
      .subscribe({
        next: (data) => {
          this.order = data;
          directive.resetForm({ menuItemId: null, quantity: 1, note: '' });
        },
        error: (err) => this.handleError(err)
      });
  }

  changeQuantity(item: OrderItemDto, change: number): void {
    if (!this.order) {
      return;
    }

    const quantity = item.quantity + change;
    if (quantity < 1 || quantity > 99) {
      return;
    }

    this.orderService.updateItem(this.order.id, item.id, { quantity, note: item.note }).subscribe({
      next: (data) => (this.order = data),
      error: (err) => this.handleError(err)
    });
  }

  removeItem(item: OrderItemDto): void {
    if (!this.order) {
      return;
    }

    this.orderService.removeItem(this.order.id, item.id).subscribe({
      next: (data) => (this.order = data),
      error: (err) => this.handleError(err)
    });
  }

  closeOrder(): void {
    if (!this.order || this.order.items.length === 0) {
      return;
    }

    if (!confirm(`Close the order and issue an invoice for ${this.order.total.toFixed(2)} ${this.currency}?`)) {
      return;
    }

    this.orderService.closeOrder(this.order.id, { paymentMethod: this.paymentControl.value }).subscribe({
      next: (data) => {
        this.invoice = data;
        this.order = null;
        this.toastr.success('Invoice issued.');
      },
      error: (err) => this.handleError(err)
    });
  }

  cancelOrder(): void {
    if (!this.order) {
      return;
    }

    if (!confirm('Cancel this order? The table will be freed.')) {
      return;
    }

    this.orderService.cancelOrder(this.order.id).subscribe({
      next: () => {
        this.toastr.success('Order cancelled.');
        this.backToMap();
      },
      error: (err) => this.handleError(err)
    });
  }

  print(): void {
    window.print();
  }

  backToMap(): void {
    this.router.navigate(['/dashboard/tables']);
  }

  private handleError(err: any): void {
    this.toastr.error(err.error?.message ?? 'Something went wrong. Please try again.');

    if (err.status === 404 || err.status === 409) {
      this.loadOrder();
    }
  }
}
import { Component, ElementRef, OnInit, ViewChild , OnDestroy} from '@angular/core';
import { FormBuilder, FormGroup, FormGroupDirective, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { ToastrService } from 'ngx-toastr';
import { TableService } from '../../services/table.service';
import { AuthService } from '../../services/auth.service';
import { TableDto,TableStatus } from '../../models/table.models';
import { interval, merge, Subscription } from 'rxjs';
import { RealtimeService } from '../../services/realtime.service';
import { Router } from '@angular/router';

const MAP_WIDTH = 160;
const MAP_HEIGHT = 100;

@Component({
  selector: 'app-tables',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule
  ],
  templateUrl: './tables.component.html',
  styleUrl: './tables.component.css'
})
export class TablesComponent implements OnInit, OnDestroy {
  @ViewChild('mapSvg') mapSvg!: ElementRef<SVGSVGElement>;

  tables: TableDto[] = [];
  selectedTable: TableDto | null = null;
  errorMessage = '';
  addError = '';
  editError = '';
  isAdmin: boolean;
  addForm: FormGroup;
  editForm: FormGroup;

  readonly mapWidth = MAP_WIDTH;
  readonly mapHeight = MAP_HEIGHT;
  readonly xScale = MAP_WIDTH / 100;

  private draggingTable: TableDto | null = null;
  private dragMoved = false;

  private refreshSubscription: Subscription | null = null;

  constructor(
    private tableService: TableService,
    private authService: AuthService,
    private toastr: ToastrService,
    private formBuilder: FormBuilder,
    private realtimeService: RealtimeService,
    private router:Router
  ) {
    this.isAdmin = this.authService.getRole() === 'Admin';

    this.addForm = this.formBuilder.group({
      number: [null, [Validators.required, Validators.min(1), Validators.max(1000)]],
      capacity: [null, [Validators.required, Validators.min(1), Validators.max(50)]]
    });

    this.editForm = this.formBuilder.group({
      number: [null, [Validators.required, Validators.min(1), Validators.max(1000)]],
      capacity: [null, [Validators.required, Validators.min(1), Validators.max(50)]]
    });
  }

   ngOnInit(): void {
    this.loadTables();

    this.refreshSubscription = merge(
    this.realtimeService.tablesChanged$,
    this.realtimeService.reservationsChanged$,
    this.realtimeService.reconnected$,
    interval(30000)
  ).subscribe(() => this.refresh());
  }

  ngOnDestroy(): void {
    this.refreshSubscription?.unsubscribe();
  }

 
private refresh(): void {
  if (this.draggingTable) {
    return;
  }
  this.loadTables();
}

 loadTables(): void {
  this.tableService.getAllTables().subscribe({
    next: (data) => {
      this.tables = data;

      if (this.selectedTable) {
        const id = this.selectedTable.id;
        this.selectedTable = data.find(t => t.id === id) ?? null;
      }
    },
    error: () => (this.errorMessage = 'Could not load tables.')
  });
}

  addTable(directive: FormGroupDirective): void {
    if (this.addForm.invalid) {
      this.addForm.markAllAsTouched();
      return;
    }

    this.addError = '';
    const { number, capacity } = this.addForm.getRawValue();

    this.tableService
      .registerTable({ number, capacity, positionX: 50, positionY: 50 })
      .subscribe({
        next: () => {
          this.toastr.success('Table added.');
          directive.resetForm();
          this.loadTables();
        },
        error: (err) => {
          this.addError = err.error?.message ?? 'Something went wrong. Please try again.';
        }
      });
  }

  selectTable(table: TableDto): void {
    this.selectedTable = table;
    this.editError = '';
    this.editForm.setValue({ number: table.number, capacity: table.capacity });
  }

  clearSelection(): void {
    this.selectedTable = null;
  }

  setStatus(status: TableStatus): void {
  if (!this.selectedTable) {
    return;
  }

  this.tableService.updateTableStatus(this.selectedTable.id, status).subscribe({
    next: () => this.loadTables(),
    error: (err) => {
      this.toastr.error(err.error?.message ?? 'Something went wrong. Please try again.');
    }
  });
}

statusLabel(table: TableDto): string {
  if (table.status === 'Occupied') {
    return 'Occupied';
  }
  return table.reservedSoon ? 'Reserved soon' : 'Available';
}

  saveTable(): void {
    if (!this.selectedTable) {
      return;
    }

    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }

    this.editError = '';
    const { number, capacity } = this.editForm.getRawValue();
    const table = this.selectedTable;

    this.tableService
      .updateTable(table.id, {
        number,
        capacity,
        positionX: table.positionX,
        positionY: table.positionY
      })
      .subscribe({
        next: () => {
          this.toastr.success('Table updated.');
          this.selectedTable = null;
          this.loadTables();
        },
        error: (err) => {
          this.editError = err.error?.message ?? 'Something went wrong. Please try again.';
        }
      });
  }

  removeTable(): void {
    if (!this.selectedTable) {
      return;
    }

    const table = this.selectedTable;

    if (!confirm(`Remove table ${table.number}?`)) {
      return;
    }

    this.tableService.deleteTable(table.id).subscribe({
      next: () => {
        this.toastr.success('Table removed.');
        this.selectedTable = null;
        this.loadTables();
      },
      error: (err) => {
        this.toastr.error(err.error?.message ?? 'Something went wrong. Please try again.');
      }
    });
  }

  startDrag(table: TableDto): void {
  this.selectTable(table);

  if (!this.isAdmin) {
    return;
  }

  this.draggingTable = table;
  this.dragMoved = false;
}

  onPointerMove(event: PointerEvent): void {
    if (!this.draggingTable) {
      return;
    }

    const position = this.toMapPosition(event);
    this.draggingTable.positionX = position.x;
    this.draggingTable.positionY = position.y;
    this.dragMoved = true;
  }

  endDrag(): void {
    if (!this.draggingTable) {
      return;
    }

    const table = this.draggingTable;
    this.draggingTable = null;

    if (!this.dragMoved) {
      return;
    }

    this.tableService
      .updateTablePosition(table.id, { positionX: table.positionX, positionY: table.positionY })
      .subscribe({
        error: (err) => {
          this.toastr.error(err.error?.message ?? 'Could not save the position.');
          this.loadTables();
        }
      });
  }

  private toMapPosition(event: PointerEvent): { x: number; y: number } {
    const svg = this.mapSvg.nativeElement;
    const matrix = svg.getScreenCTM();

    if (!matrix) {
      return { x: 0, y: 0 };
    }

    const point = svg.createSVGPoint();
    point.x = event.clientX;
    point.y = event.clientY;
    const mapPoint = point.matrixTransform(matrix.inverse());

    return {
      x: this.limit((mapPoint.x / MAP_WIDTH) * 100),
      y: this.limit((mapPoint.y / MAP_HEIGHT) * 100)
    };
  }

  private limit(value: number): number {
    const rounded = Math.round(value * 10) / 10;
    return Math.min(100, Math.max(0, rounded));
  }

  openOrder(): void {
  if (!this.selectedTable) {
    return;
  }

  this.router.navigate(['/dashboard/orders', this.selectedTable.id]);
}
}
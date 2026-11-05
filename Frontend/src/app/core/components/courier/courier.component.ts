import { DecimalPipe } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { OnlineOrderDto } from '../../models/online-order.models';
import { CURRENCY } from '../../currency';
import { forkJoin, interval, merge, Subscription } from 'rxjs';
import { OnlineOrderService } from '../../services/online-order.service';
import { ToastrService } from 'ngx-toastr';
import { RealtimeService } from '../../services/realtime.service';

@Component({
  selector: 'app-courier',
  standalone: true,
  imports: [DecimalPipe],
  templateUrl: './courier.component.html',
  styleUrl: './courier.component.css'
})
export class CourierComponent implements OnInit, OnDestroy {
  available: OnlineOrderDto[] = [];
  myDelivery: OnlineOrderDto | null = null;
  loaded = false;
  busy = false;
  errorMessage = '';
  readonly currency = CURRENCY;

  private refreshSubscription?: Subscription;

   constructor(
    private onlineOrderService: OnlineOrderService,
    private realtimeService: RealtimeService,
    private toastr: ToastrService
  ) { }



  ngOnInit(): void {
    this.load();

    this.refreshSubscription = merge(
      this.realtimeService.onlineOrdersChanged$,
      this.realtimeService.reconnected$,
      interval(30000)
    ).subscribe(() => this.load());
  }

  ngOnDestroy(): void {
    this.refreshSubscription?.unsubscribe();
  }

   load(): void {
    this.errorMessage = '';

    forkJoin({
      available: this.onlineOrderService.getAvailable(),
      mine: this.onlineOrderService.getMyDelivery()
    }).subscribe({
      next: ({ available, mine }) => {
        this.available = available;
        this.myDelivery = mine.length > 0 ? mine[0] : null;
        this.loaded = true;
      },
      error: () => (this.errorMessage = 'Failed to load deliveries. Please try again later.')
    });
  }

    take(order: OnlineOrderDto): void {
    this.busy = true;

    this.onlineOrderService.claim(order.id).subscribe({
      next: () => {
        this.busy = false;
        this.toastr.success(`Order #${order.id} is yours.`);
        this.load();
      },
      error: (err) => {
        this.busy = false;
        this.toastr.error(err.error?.message ?? 'Something went wrong. Please try again.');
        this.load();
      }
    });
  }

  deliver(order: OnlineOrderDto): void {
    if (!confirm(`Delivered order #${order.id} and collected ${order.total.toFixed(2)} ${this.currency}?`)) {
      return;
    }

    this.busy = true;

    this.onlineOrderService.deliver(order.id).subscribe({
      next: () => {
        this.busy = false;
        this.toastr.success(`Order #${order.id} delivered.`);
        this.load();
      },
      error: (err) => {
        this.busy = false;
        this.toastr.error(err.error?.message ?? 'Something went wrong. Please try again.');
        this.load();
      }
    });
  }
}


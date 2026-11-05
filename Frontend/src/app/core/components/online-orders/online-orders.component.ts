import { Component, OnDestroy, OnInit } from '@angular/core';
import { DeliveryStatus, OnlineOrderDto } from '../../models/online-order.models';
import { DatePipe, DecimalPipe } from '@angular/common';
import { CURRENCY } from '../../currency';
import { interval, merge, Subscription } from 'rxjs';
import { OnlineOrderService } from '../../services/online-order.service';
import { RealtimeService } from '../../services/realtime.service';
import { ToastrService } from 'ngx-toastr';


interface OrderAction {
  label: string;
  status: DeliveryStatus;
  danger?: boolean;
}

@Component({
  selector: 'app-online-orders',
  standalone: true,
  imports: [DatePipe,DecimalPipe],
  templateUrl: './online-orders.component.html',
  styleUrl: './online-orders.component.css'
})
export class OnlineOrdersComponent implements OnInit, OnDestroy {
orders: OnlineOrderDto[] = [];
  loaded = false;
  errorMessage = '';
  readonly currency = CURRENCY;


readonly labels: Record<DeliveryStatus, string> = {
    Received: 'New',
    Confirmed: 'Confirmed',
    Preparing: 'Preparing',
    Ready: 'Ready',
    OutForDelivery: 'On the way',
    Delivered: 'Delivered',
    PickedUp: 'Picked up',
    Cancelled: 'Cancelled'
  };

  private knownIds = new Set<number>();
  private refreshSubscription?: Subscription;

  constructor(
    private onlineOrderService: OnlineOrderService,
    private realtimeService: RealtimeService,
    private toastr: ToastrService
  ) { }



  ngOnInit(): void {
    this.loadOrders();

    this.refreshSubscription = merge(
      this.realtimeService.onlineOrdersChanged$,
      this.realtimeService.reconnected$,
      interval(30000)
    ).subscribe(() => this.loadOrders());
  }

  ngOnDestroy(): void {
   this.refreshSubscription?.unsubscribe();
  }
  
  loadOrders(): void {
    this.errorMessage = '';

    this.onlineOrderService.getActiveOrders().subscribe({
      next: (data) => {
       if(this.loaded) {
        for(const order of data) {
          if(!this.knownIds.has(order.id)) {
            this.toastr.info(`New order received: #${order.id}`, 'New Order');
          }
        }
       }
       this.knownIds = new Set(data.map(o => o.id));
        this.orders = data;
        this.loaded = true;
      },
      error: () => ( this.errorMessage = 'Failed to load orders. Please try again later.')

    });

  }
 actions(order: OnlineOrderDto): OrderAction[] {
    const cancel: OrderAction = { label: 'Cancel', status: 'Cancelled', danger: true };

    switch (order.status) {
      case 'Received':
        return [{ label: 'Confirm', status: 'Confirmed' }, { label: 'Reject', status: 'Cancelled', danger: true }];
      case 'Confirmed':
        return [{ label: 'Start preparing', status: 'Preparing' }, cancel];
      case 'Preparing':
        return [{ label: 'Mark ready', status: 'Ready' }, cancel];
      case 'Ready':
        return order.channel === 'Pickup' ? [{ label: 'Picked up', status: 'PickedUp' }, cancel] : [cancel];
      case 'OutForDelivery':
        return [cancel];  

      default:
        return [];
    }
  }

  changeStatus(order: OnlineOrderDto, action: OrderAction): void {
    if (action.status === 'Cancelled' && !confirm(`Cancel order #${order.id}?`)) {
      return;
    }

    this.onlineOrderService.changeStatus(order.id, action.status).subscribe({
      next: () => {
        this.toastr.success(`Order #${order.id}: ${this.labels[action.status]}.`);
        this.loadOrders();
      },
      error: (err) => {
        this.toastr.error(err.error?.message ?? 'Something went wrong. Please try again.');
        this.loadOrders();
      }
    });
  }
  

}

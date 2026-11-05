import { Component, OnDestroy, OnInit } from '@angular/core';
import { DeliveryStatus, OnlineOrderDto } from '../../models/online-order.models';
import { CURRENCY } from '../../currency';
import { interval, merge, Subscription } from 'rxjs';
import { OnlineOrderService } from '../../services/online-order.service';
import { RealtimeService } from '../../services/realtime.service';
import { ToastrService } from 'ngx-toastr';
import { DatePipe, DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';


const PICKUP_STEPS: DeliveryStatus[] = ['Received', 'Confirmed', 'Preparing', 'Ready', 'PickedUp'];
const DELIVERY_STEPS: DeliveryStatus[] = ['Received', 'Confirmed', 'Preparing', 'Ready', 'OutForDelivery', 'Delivered'];

@Component({
  selector: 'app-my-orders',
  standalone: true,
  imports: [DatePipe,DecimalPipe,RouterLink],
  templateUrl: './my-orders.component.html',
  styleUrl: './my-orders.component.css'
})
export class MyOrdersComponent implements OnInit, OnDestroy {
  orders: OnlineOrderDto[] = [];
  loaded = false;
  errorMessage = '';
  readonly currency = CURRENCY;

  readonly labels: Record<DeliveryStatus, string> = {
    Received: 'Received',
    Confirmed: 'Confirmed',
    Preparing: 'Preparing',
    Ready: 'Ready',
    OutForDelivery: 'Out for Delivery',
    Delivered: 'Delivered',
    PickedUp: 'Picked Up',
    Cancelled: 'Cancelled'

  };

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

    this.onlineOrderService.getMyOrders().subscribe({ 
      next:(data) => {
        this.orders = data;
        this.loaded = true;
      },
      error: () => ( this.errorMessage = 'Failed to load orders. Please try again later.')
      
    });
  }

  steps(order: OnlineOrderDto): DeliveryStatus[] { 
    return order.channel === 'Delivery' ? DELIVERY_STEPS : PICKUP_STEPS;
  }

  stepClass(order:OnlineOrderDto, step: DeliveryStatus): string {
    const steps = this.steps(order);
    const current = steps.indexOf(order.status);
    const index = steps.indexOf(step);

    if(index < current) {
      return 'done';
    }

    return index === current ? 'current' : '';

  }

  cancelOrder(order: OnlineOrderDto): void {
    
    if(!confirm('Cancel this order?')) {
      return;
    }

    this.onlineOrderService.cancelMyOrder(order.id).subscribe({ 
      next:() => {
        this.toastr.success('Order cancelled successfully.');
        this.loadOrders();
      },
      error:(err) => { 
        this.toastr.error(err.error?.message || 'Failed to cancel order. Please try again later.');
        this.loadOrders();
      }
    });

  }
}

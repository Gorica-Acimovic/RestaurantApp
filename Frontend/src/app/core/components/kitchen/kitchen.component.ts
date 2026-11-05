import { Component, OnDestroy, OnInit } from '@angular/core';
import { KitchenItemDto, KitchenTicketDto } from '../../models/kitchen.models';
import { interval, merge, Observable, Subscription } from 'rxjs';
import { KitchenService } from '../../services/kitchen.service';
import { RealtimeService } from '../../services/realtime.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-kitchen',
  standalone: true,
  imports: [],
  templateUrl: './kitchen.component.html',
  styleUrl: './kitchen.component.css'
})
export class KitchenComponent implements OnInit,OnDestroy {
  tickets: KitchenTicketDto[] =[];
  loaded = false; 
  errorMessage ='';
  now = Date.now();

  private refreshSubscription?: Subscription;
  private clockSubscription?:Subscription;
  
constructor( 
  private kitchenService:KitchenService,
  private realtimeService:RealtimeService,
  private toastr:ToastrService
) {}

 ngOnInit(): void {
   this.loadTickets();

   this.refreshSubscription = merge ( 
    this.realtimeService.kitchenChanged$,
    this.realtimeService.reconnected$,
    interval(30000)
   ).subscribe(() => this.loadTickets());

   this.clockSubscription = interval(30000).subscribe(() => (this.now = Date.now()));
  }

  ngOnDestroy(): void {
   this.refreshSubscription?.unsubscribe();
   this.clockSubscription?.unsubscribe();
  }
 

  loadTickets() :void {

      this.errorMessage = '';
      this.kitchenService.getTickets().subscribe({
        next: (data) => {
          this.tickets = data;
          this.now = Date.now();
          this.loaded = true;
        },
        error: () => (this.errorMessage = 'Failed to load tickets. Please try again later.')
        
      });

  }

  minutesWaiting(ticket:KitchenTicketDto): number {
    const minutes = Math.floor((this.now - new Date(ticket.since).getTime()) / 60000); 
    return Math.max(minutes,0);
  }

  hasNew(ticket: KitchenTicketDto) : boolean {
    return ticket.items.some(i => i.status === "New"); 
  }

  startItem( item: KitchenItemDto) : void {
    this.run(this.kitchenService.changeItemStatus(item.id,'Preparing'));
  }
  finishItem(item: KitchenItemDto): void {
    this.run(this.kitchenService.changeItemStatus(item.id, 'Ready'));
  }

  startAll(ticket: KitchenTicketDto): void {
    this.run(this.kitchenService.changeOrderStatus(ticket.orderId, 'Preparing'));
  }

  finishAll(ticket: KitchenTicketDto): void {
    this.run(this.kitchenService.changeOrderStatus(ticket.orderId, 'Ready'));
  }

  private run(request: Observable<void>): void { 
    request.subscribe({
      next: () => this.loadTickets(),
      error: (err) => {
        this.toastr.error(err.error?.message ?? 'Something went wrong. Please try again.');
        this.loadTickets();
      }
    });
  }

}

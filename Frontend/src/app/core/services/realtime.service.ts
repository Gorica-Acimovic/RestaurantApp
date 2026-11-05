import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { HUB_URL } from '../api-config';
import { AuthService } from './auth.service';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';

@Injectable({
  providedIn: 'root'
})
export class RealtimeService {
  readonly tablesChanged$ = new Subject<void>();
  readonly reservationsChanged$ = new Subject<void>();
  readonly reconnected$ = new Subject<void>();
  readonly onlineOrdersChanged$ = new Subject<void>();
  readonly kitchenChanged$ = new Subject<void>(); 
  readonly orderReady$ = new Subject<number>(); 

  private connection : HubConnection | null = null;
  private startFailed = false;

  readonly orderChanged$ = new Subject<void>();
  private joinedTables = new Set<number>();

  constructor(private authService:AuthService) { }

  connect(): void {
    if(this.connection) {
      return;
    }
    this.startFailed = false;

    this.connection = new HubConnectionBuilder()
          .withUrl(HUB_URL, {accessTokenFactory: () => this.authService.getAccessToken() ?? '' })
            .withAutomaticReconnect({
              nextRetryDelayInMilliseconds:() => 5000
          })
          .build();

    this.connection.on('TablesChanged', () => this.tablesChanged$.next());
    this.connection.on('ReservationsChanged', () => this.reservationsChanged$.next());
    this.connection.on('OrderChanged', () => this.orderChanged$.next());
    this.connection.on('OnlineOrdersChanged', () => this.onlineOrdersChanged$.next());
    this.connection.on('KitchenChanged', () => this.kitchenChanged$.next());
    this.connection.on('OrderReady', (tableNumber:number) => this.orderReady$.next(tableNumber));

    this.connection.onreconnected(() => {
      this.rejoinTables();
      this.reconnected$.next();
    });
    

          this.start();
  }
 disconnect(): void {
    this.connection?.stop();
    this.connection = null;
    this.joinedTables.clear();
  }

  private start(): void {
    const connection = this.connection;
    if (!connection) {
      return;
    }

    connection.start()
      .then(() => {
         this.rejoinTables();
        if (this.startFailed) {
          this.startFailed = false;
          this.reconnected$.next();
        }
      })
      .catch(() => {
        this.startFailed = true;
        setTimeout(() => {
          if (this.connection === connection) {
            this.start();
          }
        }, 5000);
      });
  }
   
joinTable(tableId: number): void {
  this.joinedTables.add(tableId);
  this.invokeHub('JoinTable', tableId);
}

leaveTable(tableId: number): void {
  this.joinedTables.delete(tableId);
  this.invokeHub('LeaveTable', tableId);
}

private rejoinTables(): void {
  this.joinedTables.forEach(id => this.invokeHub('JoinTable', id));
}

private invokeHub(method: string, tableId: number): void {
  if (this.connection?.state === HubConnectionState.Connected) {
    this.connection.invoke(method, tableId).catch(() => { });
  }
}


}

import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { HUB_URL } from '../api-config';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class RealtimeService {
  readonly tablesChanged$ = new Subject<void>();
  readonly reservationsChanged$ = new Subject<void>();
  readonly reconnected$ = new Subject<void>();

  private connection : HubConnection | null = null;
  private startFailed = false;

  constructor(private authService:AuthService) { }

  connect(): void {
    if(this.connection) {
      return;
    }
    this.startFailed = false;

    this.connection= new HubConnectionBuilder()
          .withUrl(HUB_URL, {accessTokenFactory: () => this.authService.getAccessToken() ?? '' })
            .withAutomaticReconnect({
              nextRetryDelayInMilliseconds:() => 5000
          })
          .build();

            this.connection.on('TablesChanged', () => this.tablesChanged$.next());
    this.connection.on('ReservationsChanged', () => this.reservationsChanged$.next());
    this.connection.onreconnected(() => this.reconnected$.next());

          this.start();
  }
 disconnect(): void {
    this.connection?.stop();
    this.connection = null;
  }

  private start(): void {
    const connection = this.connection;
    if (!connection) {
      return;
    }

    connection.start()
      .then(() => {
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
   



}

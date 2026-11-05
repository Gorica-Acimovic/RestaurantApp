import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { API_URL } from '../api-config';
import { Observable } from 'rxjs';
import { KitchenItemStatus, KitchenTicketDto } from '../models/kitchen.models';

@Injectable({
  providedIn: 'root'
})
export class KitchenService {

  constructor(private http:HttpClient) { }

  getTickets(): Observable<KitchenTicketDto[]> {
    return this.http.get<KitchenTicketDto[]>(`${API_URL}/kitchen/tickets`);
  }

  changeItemStatus(itemId: number, status :KitchenItemStatus) : Observable<void> {
    return this.http.post<void>(`${API_URL}/kitchen/items/${itemId}/status`, {status});
  }

  changeOrderStatus(orderId: number, status:KitchenItemStatus) : Observable<void> {
    return this.http.post<void>(`${API_URL}/kitchen/orders/${orderId}/status`,{status});
  }
}

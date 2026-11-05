import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { CreateOnlineOrderRequest, OnlineOrderDto, OnlineOrderSettings, DeliveryStatus } from '../models/online-order.models';
import { API_URL } from '../api-config';

@Injectable({
  providedIn: 'root'
})
export class OnlineOrderService {

  constructor(private http: HttpClient) { }

 getSettings(): Observable<OnlineOrderSettings> {
    return this.http.get<OnlineOrderSettings>(`${API_URL}/OnlineOrders/settings`);
  }

   createOrder(request: CreateOnlineOrderRequest): Observable<OnlineOrderDto> {
    return this.http.post<OnlineOrderDto>(`${API_URL}/OnlineOrders`, request);
  }

  getMyOrders(): Observable<OnlineOrderDto[]> {
    return this.http.get<OnlineOrderDto[]>(`${API_URL}/OnlineOrders/mine`);
  }

  cancelMyOrder(orderId: number): Observable<OnlineOrderDto> {
    return this.http.post<OnlineOrderDto>(`${API_URL}/OnlineOrders/${orderId}/cancel`, {});
  }

  getActiveOrders(): Observable<OnlineOrderDto[]> {
  return this.http.get<OnlineOrderDto[]>(`${API_URL}/OnlineOrders/active`);
}

changeStatus(orderId: number, status: DeliveryStatus): Observable<OnlineOrderDto> {
  return this.http.post<OnlineOrderDto>(`${API_URL}/OnlineOrders/${orderId}/status`, { status });
}
getAvailable(): Observable<OnlineOrderDto[]> {
  return this.http.get<OnlineOrderDto[]>(`${API_URL}/OnlineOrders/available`);
}

getMyDelivery(): Observable<OnlineOrderDto[]> {
  return this.http.get<OnlineOrderDto[]>(`${API_URL}/OnlineOrders/my-delivery`);
}

claim(orderId: number): Observable<OnlineOrderDto> {
  return this.http.post<OnlineOrderDto>(`${API_URL}/OnlineOrders/${orderId}/claim`, {});
}

deliver(orderId: number): Observable<OnlineOrderDto> {
  return this.http.post<OnlineOrderDto>(`${API_URL}/OnlineOrders/${orderId}/deliver`, {});
}

}

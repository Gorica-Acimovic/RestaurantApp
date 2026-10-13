import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AddOrderItemRequest, CloseOrderRequest, InvoiceDto, OpenOrderRequest, OrderDto, UpdateOrderItemRequest } from '../models/order.models';
import { Observable } from 'rxjs';
import { API_URL } from '../api-config';

@Injectable({
  providedIn: 'root'
})
export class OrderService {

  constructor(private http:HttpClient) { }

  getOpenOrderForTable(tableId: number): Observable<OrderDto> {
    return this.http.get<OrderDto>(`${API_URL}/Orders/table/${tableId}`);
  }

  openOrder(request: OpenOrderRequest): Observable<OrderDto> {
    return this.http.post<OrderDto>(`${API_URL}/Orders`, request);
  }

  addItem(orderId: number, request: AddOrderItemRequest): Observable<OrderDto> {
    return this.http.post<OrderDto>(`${API_URL}/Orders/${orderId}/items`, request);
  }

  updateItem(orderId: number, itemId: number, request: UpdateOrderItemRequest): Observable<OrderDto> {
    return this.http.put<OrderDto>(`${API_URL}/Orders/${orderId}/items/${itemId}`, request);
  }

  removeItem(orderId: number, itemId: number): Observable<OrderDto> {
    return this.http.delete<OrderDto>(`${API_URL}/Orders/${orderId}/items/${itemId}`);
  }

  closeOrder(orderId: number, request: CloseOrderRequest): Observable<InvoiceDto> {
    return this.http.post<InvoiceDto>(`${API_URL}/Orders/${orderId}/close`, request);
  }

  cancelOrder(orderId: number): Observable<void> {
    return this.http.post<void>(`${API_URL}/Orders/${orderId}/cancel`, {});
  }

  getInvoice(orderId: number): Observable<InvoiceDto> {
    return this.http.get<InvoiceDto>(`${API_URL}/Orders/${orderId}/invoice`);
  }
}

import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { SaveTableRequest, TableDto, TableStatus, UpdateTablePositionRequest } from '../models/table.models';
import { Observable } from 'rxjs';
import { API_URL } from '../api-config';

@Injectable({
  providedIn: 'root'
})
export class TableService {

  constructor(private http: HttpClient) { }

  getAllTables():Observable<TableDto[]>{
    return this.http.get<TableDto[]>(`${API_URL}/restaurantTables`);
  }

  registerTable(request: SaveTableRequest):Observable<TableDto>{
    return this.http.post<TableDto>(`${API_URL}/restaurantTables`, request);
  }

  updateTablePosition(tableId: number, request: UpdateTablePositionRequest):Observable<void>{
    return this.http.put<void>(`${API_URL}/restaurantTables/${tableId}/position`, request);
  }
  updateTable(tableId: number, request: SaveTableRequest):Observable<void>{
    return this.http.put<void>(`${API_URL}/restaurantTables/${tableId}`, request);
  }
  updateTableStatus(tableId: number, status: TableStatus): Observable<void> {
  return this.http.put<void>(`${API_URL}/restaurantTables/${tableId}/status`, { status });
}
  deleteTable(tableId: number):Observable<void>{
    return this.http.delete<void>(`${API_URL}/restaurantTables/${tableId}`);
  }

}

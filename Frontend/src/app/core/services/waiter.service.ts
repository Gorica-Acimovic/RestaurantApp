import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { RegisterWaiterRequest, UpdateWaiterRequest, UserDto } from '../models/user.models';
import { Observable } from 'rxjs';
import { API_URL } from '../api-config';

@Injectable({
  providedIn: 'root'
})
export class WaiterService {

  constructor(private http: HttpClient) { }

  registerWaiter(request: RegisterWaiterRequest): Observable<UserDto> {
  return this.http.post<UserDto>(`${API_URL}/waiters`, request);
}

getAllWaiters(): Observable<UserDto[]> {
  return this.http.get<UserDto[]>(`${API_URL}/waiters`);
}

setActive(id: number, isActive: boolean): Observable<void> {
  const action = isActive ? 'activate' : 'deactivate';
  return this.http.put<void>(`${API_URL}/waiters/${id}/${action}`, {});
}
updateWaiter(id: number, request: UpdateWaiterRequest): Observable<void> {
  return this.http.put<void>(`${API_URL}/waiters/${id}`, request);
}
}

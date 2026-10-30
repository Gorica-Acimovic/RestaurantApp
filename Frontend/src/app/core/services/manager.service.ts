import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {API_URL} from '../api-config';
import { UpdateManagerRequest, UserDto } from '../models/user.models';
import { RegisterRequest } from '../models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class ManagerService {

  constructor(private http: HttpClient) { }

  getAllManagers(): Observable<UserDto[]> {
    return this.http.get<UserDto[]>(`${API_URL}/managers`);
  }

  registerManager(request: RegisterRequest): Observable<UserDto> {
  return this.http.post<UserDto>(`${API_URL}/managers`, request);
}

setActive(id: number, isActive: boolean): Observable<void> {
  const action = isActive ? 'activate' : 'deactivate';
  return this.http.put<void>(`${API_URL}/managers/${id}/${action}`, {});
}

updateManager(id: number, request: UpdateManagerRequest): Observable<void> {
  return this.http.put<void>(`${API_URL}/managers/${id}`, request);
}

}

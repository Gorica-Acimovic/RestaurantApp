import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ChangePasswordRequest, DeactivateAccountRequest, UserDto } from '../models/user.models';
import { API_URL } from '../api-config';

@Injectable({
  providedIn: 'root'
})
export class ProfileService {

  constructor(private http: HttpClient) { }

  getProfile(): Observable<UserDto> {
    return this.http.get<UserDto>(`${API_URL}/Profile`);
  }

  changePassword(request: ChangePasswordRequest):Observable<void> {
    return this.http.put<void> (`${API_URL}/Profile/change-password`, request);
  }

  deactivate(request:DeactivateAccountRequest):Observable<void> {
    return this.http.put<void>(`${API_URL}/Profile/deactivate`, request);
  }
}

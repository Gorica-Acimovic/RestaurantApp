import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import {API_URL} from '../api-config';
import { LoginRequest, LoginResponse, PinLoginRequest, RegisterRequest, Role } from '../models/auth.models';
import { Observable, tap } from 'rxjs';
const SESSION_STORAGE_KEY = 'session';

@Injectable({
  providedIn: 'root'
})
export class AuthService {

  constructor(private http: HttpClient) { }


  login(request:LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${API_URL}/auth/login`, request)
    .pipe(tap((response) => this.saveSession(response)));
  }

  register(request:RegisterRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${API_URL}/auth/register`, request)
    .pipe(tap((response) => this.saveSession(response)));
  }

  loginWithPin(request: PinLoginRequest): Observable<LoginResponse> {
  return this.http.post<LoginResponse>(`${API_URL}/auth/pin-login`, request)
    .pipe(tap((response) => this.saveSession(response)));
}

logout(): void {
    const session = this.getSession(); 
    if (session) {
      this.http
        .post(`${API_URL}/auth/logout`, { refreshToken: session.refreshToken })
        .subscribe();
    }
    sessionStorage.removeItem(SESSION_STORAGE_KEY); 
  }

  getSession(): LoginResponse | null {
    const text = sessionStorage.getItem(SESSION_STORAGE_KEY);
    return text ? (JSON.parse(text) as LoginResponse) : null;
  }

  getAccessToken(): string | null {
    return this.getSession()?.accessToken ?? null;
  }

  getRole(): Role | null {
    return this.getSession()?.role ?? null;
  }

  isLoggedIn(): boolean {
    return this.getSession() !== null;
  }

  refresh(): Observable<LoginResponse> {
  const session = this.getSession();

  return this.http
    .post<LoginResponse>(`${API_URL}/auth/refresh`, { refreshToken: session?.refreshToken })
    .pipe(tap((response) => this.saveSession(response)));
}

  private saveSession(response: LoginResponse): void {
    sessionStorage.setItem(SESSION_STORAGE_KEY, JSON.stringify(response));
  }

  
}
export type Role = 'Guest' | 'Waiter' | 'Manager' | 'Admin' | 'Kitchen' | 'Courier';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface PinLoginRequest {
    pin: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
}

export interface LoginResponse {
 accessToken: string;
 refreshToken: string;
 expiresAt: string;
 firstName: string;
 lastName: string;
 role: Role;
}
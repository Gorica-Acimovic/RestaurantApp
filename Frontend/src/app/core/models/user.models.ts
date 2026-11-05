import { Role } from "./auth.models";

export interface UserDto {
    id: number;
    firstName: string;
    lastName: string;
    email: string;
    role: Role;
    isActive: boolean;
}

export interface RegisterWaiterRequest{
    firstName: string;
    lastName: string;
    email: string;
    pin: string;
    role:Role;
}

export interface ChangePasswordRequest {
    currentPassword: string;
    newPassword:string;
}

export interface DeactivateAccountRequest {
    password: string | null;
    pin: string | null;
}

export interface UpdateWaiterRequest {
    firstName: string;
    lastName: string;
    email: string;
    pin: string | null;
}

export interface UpdateManagerRequest {
    firstName: string;
    lastName: string;
    email: string;
}
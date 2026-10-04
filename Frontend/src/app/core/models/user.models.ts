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
}
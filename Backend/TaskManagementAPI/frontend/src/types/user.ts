export enum Role {
  User = 1,
  Admin = 2
}

export interface User {
  id: number;
  username: string;
  email: string;
  role: Role;
  fullName: string;
  createdAt: string;
  isActive: boolean;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  username: string;
  email: string;
  role: Role;
  fullName: string;
}

export interface CreateUserDto {
  username: string;
  email: string;
  password: string;
  role: Role;
  fullName: string;
}

export interface UpdateUserDto {
  email: string;
  role: Role;
  fullName: string;
  isActive: boolean;
}



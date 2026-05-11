export interface RegisterRequest
{
    username: string;
    email: string;
    password?: string; 
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  success: boolean;
  message: string;
  token?: string;
  user: User;
}

export interface User{
    userid: string;
    username: string;
    email: string;
    role: string;
}
export interface RegisterRequest
{
    username: string;
    email: string;
    password?: string; 
}

export interface VerifyRequest{
  email: string;
  code:string
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  success: boolean;
  message: string;
  token?: string;
  userdto: User;
}



export interface updateProfileRequest {
  username: string;
  email: string;
}

export interface User{
    userid: string;
    username: string;
    email: string;
    role: string;
}



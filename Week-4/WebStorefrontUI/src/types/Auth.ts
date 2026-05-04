// This one file, can store multiple types for us.
// If you choose to do a strict 1 file per type, thats fine too!

// Mirrors LoginDTO from the API
export interface LoginRequest {
    username: string;
    password: string;
}

// Mirrors TokenResponseDTO from the API
export interface LoginResponse {
    token: string;
    expiresAt: string; 
}

// This class models what we decode out of the JWT for our use on the client side
// Field names match the .NET claim types that are stored in the token
export interface AuthUser {
    userId: number;
    username: string;
    role: 'User' | 'Admin'; 
}
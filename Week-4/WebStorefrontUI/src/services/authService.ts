import { api } from "./api";
import type { LoginRequest, LoginResponse } from "../types/Auth";

export const authService = {

    // Login function
    async login(creds: LoginRequest): Promise<LoginResponse> {
        const response = await api.post<LoginResponse>('/Auth/login', creds);
        return response.data;
    }


}
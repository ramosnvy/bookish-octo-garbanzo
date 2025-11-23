import { LoginRequest, LoginResponse, UserDto } from "../models";
import { api } from "../api/client";

export const AuthService = {
  async login(email: string, password: string): Promise<LoginResponse> {
    const payload: LoginRequest = { email, password };
    const { data } = await api.post<LoginResponse>("/auth/login", payload);
    return data;
  },
  async me(token: string): Promise<UserDto> {
    const { data } = await api.get<UserDto>("/auth/me", {
      headers: {
        Authorization: `Bearer ${token}`,
      },
    });
    return data;
  },
};

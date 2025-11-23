import { CreateUserRequest, UpdateUserRequest, UserDto } from "../models";
import { api } from "../api/client";

export const UserService = {
  async getAll(empresaId?: number | null): Promise<UserDto[]> {
    const params = empresaId === undefined ? undefined : { empresaId };
    const { data } = await api.get<UserDto[]>("/users", { params });
    return data;
  },
  async create(payload: CreateUserRequest): Promise<UserDto> {
    const { data } = await api.post<UserDto>("/users", payload);
    return data;
  },
  async update(id: number, payload: UpdateUserRequest): Promise<UserDto> {
    const { data } = await api.put<UserDto>(`/users/${id}`, payload);
    return data;
  },
  async remove(id: number): Promise<void> {
    await api.delete(`/users/${id}`);
  },
};

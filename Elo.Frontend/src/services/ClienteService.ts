import { ClienteDto, CreateClienteRequest } from "../models";
import { api } from "../api/client";

export const ClienteService = {
  async getAll(empresaId?: number | null): Promise<ClienteDto[]> {
    const params = empresaId !== null && empresaId !== undefined ? { empresaId } : undefined;
    const { data } = await api.get<ClienteDto[]>("/clientes", { params });
    return data;
  },
  async create(payload: CreateClienteRequest): Promise<ClienteDto> {
    const { data } = await api.post<ClienteDto>("/clientes", payload);
    return data;
  },
  async update(id: number, payload: CreateClienteRequest): Promise<ClienteDto> {
    const { data } = await api.put<ClienteDto>(`/clientes/${id}`, payload);
    return data;
  },
};

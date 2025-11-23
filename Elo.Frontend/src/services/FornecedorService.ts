import { FornecedorDto, CreateFornecedorRequest } from "../models";
import { api } from "../api/client";

export const FornecedorService = {
  async getAll(empresaId?: number | null): Promise<FornecedorDto[]> {
    const params = empresaId !== null && empresaId !== undefined ? { empresaId } : undefined;
    const { data } = await api.get<FornecedorDto[]>("/fornecedores", { params });
    return data;
  },
  async create(payload: CreateFornecedorRequest): Promise<FornecedorDto> {
    const { data } = await api.post<FornecedorDto>("/fornecedores", payload);
    return data;
  },
  async update(id: number, payload: CreateFornecedorRequest): Promise<FornecedorDto> {
    const { data } = await api.put<FornecedorDto>(`/fornecedores/${id}`, payload);
    return data;
  },
};

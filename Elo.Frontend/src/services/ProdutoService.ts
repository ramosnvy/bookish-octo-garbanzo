import { CreateProdutoRequest, ProdutoDto } from "../models";
import { api } from "../api/client";

export const ProdutoService = {
  async getAll(empresaId?: number | null): Promise<ProdutoDto[]> {
    const params = empresaId !== null && empresaId !== undefined ? { empresaId } : undefined;
    const { data } = await api.get<ProdutoDto[]>("/produtos", { params });
    return data;
  },
  async create(payload: CreateProdutoRequest): Promise<ProdutoDto> {
    const { data } = await api.post<ProdutoDto>("/produtos", payload);
    return data;
  },
  async update(id: number, payload: CreateProdutoRequest): Promise<ProdutoDto> {
    const { data } = await api.put<ProdutoDto>(`/produtos/${id}`, payload);
    return data;
  },
  async remove(id: number): Promise<void> {
    await api.delete(`/produtos/${id}`);
  },
};

import { api } from "../api/client";
import { FornecedorCategoriaDto } from "../models";

export const FornecedorCategoriaService = {
  async getAll(empresaId?: number | null): Promise<FornecedorCategoriaDto[]> {
    const params = empresaId !== null && empresaId !== undefined ? { empresaId } : undefined;
    const { data } = await api.get<FornecedorCategoriaDto[]>("/fornecedorcategorias", { params });
    return data;
  },

  async create(payload: Omit<FornecedorCategoriaDto, "id">): Promise<FornecedorCategoriaDto> {
    const { data } = await api.post<FornecedorCategoriaDto>("/fornecedorcategorias", payload);
    return data;
  },

  async update(id: number, payload: Omit<FornecedorCategoriaDto, "id">): Promise<FornecedorCategoriaDto> {
    const { data } = await api.put<FornecedorCategoriaDto>(`/fornecedorcategorias/${id}`, { id, ...payload });
    return data;
  },

  async delete(id: number): Promise<void> {
    await api.delete(`/fornecedorcategorias/${id}`);
  },
};

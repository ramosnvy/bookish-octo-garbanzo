import { CreateEmpresaRequest, EmpresaDto, UpdateEmpresaRequest } from "../models";
import { api } from "../api/client";

export const EmpresaService = {
  async getAll(): Promise<EmpresaDto[]> {
    const { data } = await api.get<EmpresaDto[]>("/empresas");
    return data;
  },
  async create(payload: CreateEmpresaRequest): Promise<EmpresaDto> {
    const { data } = await api.post<EmpresaDto>("/empresas", payload);
    return data;
  },
  async update(id: number, payload: UpdateEmpresaRequest): Promise<EmpresaDto> {
    const { data } = await api.put<EmpresaDto>(`/empresas/${id}`, payload);
    return data;
  },
};

import {
  CreateHistoriaStatusConfigRequest,
  HistoriaStatusConfigDto,
  UpdateHistoriaStatusConfigRequest,
} from "../models";
import { api } from "../api/client";

const resourcePath = "/historia-status";

const buildEmpresaConfig = (empresaId?: number | null) =>
  empresaId === undefined || empresaId === null ? undefined : { params: { empresaId } };

export const HistoriaStatusService = {
  async getAll(empresaId?: number | null): Promise<HistoriaStatusConfigDto[]> {
    const config = buildEmpresaConfig(empresaId);
    const { data } = await api.get<HistoriaStatusConfigDto[]>(resourcePath, config);
    return data;
  },

  async create(
    payload: CreateHistoriaStatusConfigRequest,
    empresaId?: number | null
  ): Promise<HistoriaStatusConfigDto> {
    const config = buildEmpresaConfig(empresaId);
    const { data } = await api.post<HistoriaStatusConfigDto>(resourcePath, payload, config);
    return data;
  },

  async update(
    id: number,
    payload: UpdateHistoriaStatusConfigRequest,
    empresaId?: number | null
  ): Promise<HistoriaStatusConfigDto> {
    const config = buildEmpresaConfig(empresaId);
    const { data } = await api.put<HistoriaStatusConfigDto>(`${resourcePath}/${id}`, payload, config);
    return data;
  },

  async remove(id: number, empresaId?: number | null): Promise<void> {
    const config = buildEmpresaConfig(empresaId);
    await api.delete(`${resourcePath}/${id}`, config);
  },
};

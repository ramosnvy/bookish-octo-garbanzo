import {
  CreateHistoriaTipoConfigRequest,
  HistoriaTipoConfigDto,
  UpdateHistoriaTipoConfigRequest,
} from "../models";
import { api } from "../api/client";

const resourcePath = "/historia-tipos";

const buildEmpresaConfig = (empresaId?: number | null) =>
  empresaId === undefined || empresaId === null ? undefined : { params: { empresaId } };

export const HistoriaTipoService = {
  async getAll(empresaId?: number | null): Promise<HistoriaTipoConfigDto[]> {
    const config = buildEmpresaConfig(empresaId);
    const { data } = await api.get<HistoriaTipoConfigDto[]>(resourcePath, config);
    return data;
  },

  async create(
    payload: CreateHistoriaTipoConfigRequest,
    empresaId?: number | null
  ): Promise<HistoriaTipoConfigDto> {
    const config = buildEmpresaConfig(empresaId);
    const { data } = await api.post<HistoriaTipoConfigDto>(resourcePath, payload, config);
    return data;
  },

  async update(
    id: number,
    payload: UpdateHistoriaTipoConfigRequest,
    empresaId?: number | null
  ): Promise<HistoriaTipoConfigDto> {
    const config = buildEmpresaConfig(empresaId);
    const { data } = await api.put<HistoriaTipoConfigDto>(`${resourcePath}/${id}`, payload, config);
    return data;
  },

  async remove(id: number, empresaId?: number | null): Promise<void> {
    const config = buildEmpresaConfig(empresaId);
    await api.delete(`${resourcePath}/${id}`, config);
  },
};

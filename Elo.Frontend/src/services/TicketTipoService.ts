import {
  CreateTicketTipoConfigRequest,
  TicketTipoConfigDto,
  UpdateTicketTipoConfigRequest,
} from "../models";
import { api } from "../api/client";

const resourcePath = "/ticket-tipos";

const buildEmpresaConfig = (empresaId?: number | null) =>
  empresaId === undefined || empresaId === null ? undefined : { params: { empresaId } };

export const TicketTipoService = {
  async getAll(empresaId?: number | null): Promise<TicketTipoConfigDto[]> {
    const config = buildEmpresaConfig(empresaId);
    const { data } = await api.get<TicketTipoConfigDto[]>(resourcePath, config);
    return data;
  },

  async create(
    payload: CreateTicketTipoConfigRequest,
    empresaId?: number | null
  ): Promise<TicketTipoConfigDto> {
    const config = buildEmpresaConfig(empresaId);
    const { data } = await api.post<TicketTipoConfigDto>(resourcePath, payload, config);
    return data;
  },

  async update(
    id: number,
    payload: UpdateTicketTipoConfigRequest,
    empresaId?: number | null
  ): Promise<TicketTipoConfigDto> {
    const config = buildEmpresaConfig(empresaId);
    const { data } = await api.put<TicketTipoConfigDto>(`${resourcePath}/${id}`, payload, config);
    return data;
  },

  async remove(id: number, empresaId?: number | null): Promise<void> {
    const config = buildEmpresaConfig(empresaId);
    await api.delete(`${resourcePath}/${id}`, config);
  },
};

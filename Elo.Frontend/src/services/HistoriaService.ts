import {
  CreateHistoriaMovimentacaoRequest,
  CreateHistoriaRequest,
  HistoriaDto,
  HistoriaStatus,
  HistoriaTipo,
  UpdateHistoriaRequest,
} from "../models";
import { api } from "../api/client";

export interface HistoriaFilter {
  status?: HistoriaStatus;
  tipo?: HistoriaTipo;
  clienteId?: number;
  produtoId?: number;
  usuarioResponsavelId?: number;
  dataInicio?: string;
  dataFim?: string;
  empresaId?: number | null;
}

export const HistoriaService = {
  async getAll(filters: HistoriaFilter = {}): Promise<HistoriaDto[]> {
    const params = { ...filters };
    if (params.empresaId === undefined || params.empresaId === null) {
      delete params.empresaId;
    }
    const { data } = await api.get<HistoriaDto[]>("/historias", { params });
    return data;
  },

  async create(payload: CreateHistoriaRequest): Promise<HistoriaDto> {
    const { data } = await api.post<HistoriaDto>("/historias", payload);
    return data;
  },

  async update(id: number, payload: UpdateHistoriaRequest): Promise<HistoriaDto> {
    const { data } = await api.put<HistoriaDto>(`/historias/${id}`, payload);
    return data;
  },

  async remove(id: number): Promise<void> {
    await api.delete(`/historias/${id}`);
  },

  async adicionarMovimentacao(id: number, payload: CreateHistoriaMovimentacaoRequest): Promise<HistoriaDto> {
    const { data } = await api.post<HistoriaDto>(`/historias/${id}/movimentacoes`, payload);
    return data;
  },
};

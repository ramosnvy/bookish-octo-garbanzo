import { CreateHistoriaMovimentacaoRequest, CreateHistoriaRequest, HistoriaDto, UpdateHistoriaRequest } from "../models";
import { api } from "../api/client";

export interface HistoriaFilter {
  statusId?: number;
  tipoId?: number;
  clienteId?: number;
  produtoId?: number;
  usuarioResponsavelId?: number;
  dataInicio?: string;
  dataFim?: string;
  empresaId?: number | null;
}

const buildEmpresaConfig = (empresaId?: number | null) =>
  empresaId === undefined || empresaId === null ? undefined : { params: { empresaId } };

const normalizeHistoriaDto = (historia: HistoriaDto): HistoriaDto => ({
  ...historia,
  usuarioResponsavelNome: historia.usuarioResponsavelNome ?? "",
  movimentacoes: historia.movimentacoes ?? [],
  produtos: historia.produtos ?? [],
});

const normalizeResponse = (dto: HistoriaDto) => normalizeHistoriaDto(dto);

export const HistoriaService = {
  async getAll(filters: HistoriaFilter = {}): Promise<HistoriaDto[]> {
    const params: Record<string, unknown> = { ...filters };
    Object.keys(params).forEach((key) => {
      if (params[key] === undefined || params[key] === null) {
        delete params[key];
      }
    });
    const { data } = await api.get<HistoriaDto[]>("/historias", { params });
    return data.map(normalizeResponse);
  },

  async create(payload: CreateHistoriaRequest, empresaId?: number | null): Promise<HistoriaDto> {
    const config = buildEmpresaConfig(empresaId);
    const { data } = await api.post<HistoriaDto>("/historias", payload, config);
    return normalizeResponse(data);
  },

  async update(id: number, payload: UpdateHistoriaRequest, empresaId?: number | null): Promise<HistoriaDto> {
    const config = buildEmpresaConfig(empresaId);
    const { data } = await api.put<HistoriaDto>(`/historias/${id}`, payload, config);
    return normalizeResponse(data);
  },

  async remove(id: number, empresaId?: number | null): Promise<void> {
    const config = buildEmpresaConfig(empresaId);
    await api.delete(`/historias/${id}`, config);
  },

  async adicionarMovimentacao(
    id: number,
    payload: CreateHistoriaMovimentacaoRequest,
    empresaId?: number | null
  ): Promise<HistoriaDto> {
    const config = buildEmpresaConfig(empresaId);
    const { data } = await api.post<HistoriaDto>(`/historias/${id}/movimentacoes`, payload, config);
    return normalizeResponse(data);
  },
};

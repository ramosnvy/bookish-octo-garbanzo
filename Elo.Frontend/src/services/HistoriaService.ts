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

const buildEmpresaConfig = (empresaId?: number | null) =>
  empresaId === undefined || empresaId === null ? undefined : { params: { empresaId } };

const mapEnumValue = <T extends Record<string | number, number | string>>(
  value: string | number,
  enumObj: T
) => (typeof value === "string" ? (enumObj[value as keyof T] as number | string) ?? value : value);

const normalizeHistoriaDto = (historia: HistoriaDto): HistoriaDto => ({
  ...historia,
  status: mapEnumValue(historia.status, HistoriaStatus) as HistoriaStatus,
  tipo: mapEnumValue(historia.tipo, HistoriaTipo) as HistoriaTipo,
  movimentacoes: historia.movimentacoes.map((mov) => ({
    ...mov,
    statusAnterior: mapEnumValue(mov.statusAnterior, HistoriaStatus) as HistoriaStatus,
    statusNovo: mapEnumValue(mov.statusNovo, HistoriaStatus) as HistoriaStatus,
  })),
});

const normalizeResponse = (dto: HistoriaDto) => normalizeHistoriaDto(dto);

export const HistoriaService = {
  async getAll(filters: HistoriaFilter = {}): Promise<HistoriaDto[]> {
    const params = { ...filters };
    if (params.empresaId === undefined || params.empresaId === null) {
      delete params.empresaId;
    }
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

import {
  CreateRespostaTicketRequest,
  CreateTicketRequest,
  TicketDto,
  TicketPrioridade,
  TicketStatus,
  UpdateTicketRequest,
} from "../models";
import { api } from "../api/client";

export interface TicketFilter {
  status?: TicketStatus;
  tipoId?: number;
  prioridade?: TicketPrioridade;
  clienteId?: number;
  usuarioAtribuidoId?: number;
  dataAberturaInicio?: string;
  dataAberturaFim?: string;
  empresaId?: number | null;
}

const buildEmpresaConfig = (empresaId?: number | null) =>
  empresaId === undefined || empresaId === null ? undefined : { params: { empresaId } };

const mapEnumValue = <T extends Record<string | number, number | string>>(
  value: string | number,
  enumObj: T
): number | string => (typeof value === "string" ? (enumObj[value as keyof T] as number | string) ?? value : value);

const normalizeTicketDto = (ticket: TicketDto): TicketDto => ({
  ...ticket,
  status: mapEnumValue(ticket.status, TicketStatus) as TicketStatus,
  prioridade: mapEnumValue(ticket.prioridade, TicketPrioridade) as TicketPrioridade,
  respostas: ticket.respostas.map((resp) => ({
    ...resp,
    isInterna: Boolean(resp.isInterna),
  })),
});

export const TicketService = {
  async getAll(filters: TicketFilter = {}): Promise<TicketDto[]> {
    const params = { ...filters };
    if (params.empresaId === undefined || params.empresaId === null) {
      delete params.empresaId;
    }
    const { data } = await api.get<TicketDto[]>("/tickets", { params });
    return data.map(normalizeTicketDto);
  },

  async create(payload: CreateTicketRequest, empresaId?: number | null): Promise<TicketDto> {
    const config = buildEmpresaConfig(empresaId);
    const { data } = await api.post<TicketDto>("/tickets", payload, config);
    return normalizeTicketDto(data);
  },

  async update(id: number, payload: UpdateTicketRequest, empresaId?: number | null): Promise<TicketDto> {
    const config = buildEmpresaConfig(empresaId);
    const { data } = await api.put<TicketDto>(`/tickets/${id}`, payload, config);
    return normalizeTicketDto(data);
  },

  async remove(id: number, empresaId?: number | null): Promise<void> {
    const config = buildEmpresaConfig(empresaId);
    await api.delete(`/tickets/${id}`, config);
  },

  async respond(id: number, payload: CreateRespostaTicketRequest, empresaId?: number | null): Promise<TicketDto> {
    const config = buildEmpresaConfig(empresaId);
    const { data } = await api.post<TicketDto>(`/tickets/${id}/respostas`, payload, config);
    return normalizeTicketDto(data);
  },
};

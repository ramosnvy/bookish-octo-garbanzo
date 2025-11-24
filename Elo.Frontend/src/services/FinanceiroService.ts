import {
  ContaPagarDto,
  ContaReceberDto,
  CreateContaPagarRequest,
  CreateContaReceberRequest,
  UpdateContaPagarRequest,
  UpdateContaReceberRequest,
} from "../models";
import { api } from "../api/client";

export const ContasPagarService = {
  async getAll(params: { empresaId?: number | null; status?: number; dataInicial?: string; dataFinal?: string } = {}): Promise<ContaPagarDto[]> {
    const { data } = await api.get<ContaPagarDto[]>("/financeiro/contas-pagar", { params });
    return data;
  },
  async create(payload: CreateContaPagarRequest): Promise<ContaPagarDto> {
    const { data } = await api.post<ContaPagarDto>("/financeiro/contas-pagar", payload);
    return data;
  },
  async update(id: number, payload: UpdateContaPagarRequest): Promise<ContaPagarDto> {
    const { data } = await api.put<ContaPagarDto>(`/financeiro/contas-pagar/${id}`, payload);
    return data;
  },
  async remove(id: number): Promise<void> {
    await api.delete(`/financeiro/contas-pagar/${id}`);
  },
};

export const ContasReceberService = {
  async getAll(params: { empresaId?: number | null; status?: number; dataInicial?: string; dataFinal?: string } = {}): Promise<ContaReceberDto[]> {
    const { data } = await api.get<ContaReceberDto[]>("/financeiro/contas-receber", { params });
    return data;
  },
  async create(payload: CreateContaReceberRequest): Promise<ContaReceberDto> {
    const { data } = await api.post<ContaReceberDto>("/financeiro/contas-receber", payload);
    return data;
  },
  async update(id: number, payload: UpdateContaReceberRequest): Promise<ContaReceberDto> {
    const { data } = await api.put<ContaReceberDto>(`/financeiro/contas-receber/${id}`, payload);
    return data;
  },
  async updateParcelaStatus(
    contaId: number,
    parcelaId: number,
    payload: UpdateContaReceberParcelaStatusRequest
  ): Promise<ContaParcelaDto> {
    const { data } = await api.put<ContaParcelaDto>(
      `/financeiro/contas-receber/${contaId}/parcelas/${parcelaId}/status`,
      payload
    );
    return data;
  },
  async remove(id: number): Promise<void> {
    await api.delete(`/financeiro/contas-receber/${id}`);
  },
};

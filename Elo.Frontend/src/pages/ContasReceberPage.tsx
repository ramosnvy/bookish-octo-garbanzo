import { useEffect, useMemo, useState } from "react";
import AppLayout from "../components/AppLayout";
import {
  ContaReceberDto,
  CreateContaReceberRequest,
  UpdateContaReceberRequest,
  ContaStatus,
  FormaPagamento,
  ClienteDto,
  ContaFinanceiroItemInput,
  UpdateContaReceberParcelaStatusRequest,
} from "../models";
import { ContasReceberService } from "../services/FinanceiroService";
import { ClienteService } from "../services/ClienteService";
import { useAuth } from "../context/AuthContext";
import { getApiErrorMessage } from "../utils/apiError";

const weekdayLabels = ["Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb"];

const formatDateKey = (date: Date) =>
  `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, "0")}-${String(date.getDate()).padStart(2, "0")}`;

type ContaFinanceiroItemForm = {
  valor?: number;
  descricao: string;
};

type ContaReceberFormState = Omit<CreateContaReceberRequest, "itens"> & {
  itens: ContaFinanceiroItemForm[];
};

type ParcelEntry = {
  key: string;
  contaId: number;
  parcelaId?: number;
  contaDescricao: string;
  clienteNome: string;
  parcelaNumero: number;
  totalParcelas: number;
  valor: number;
  status: ContaStatus;
  date: Date;
};

const statusOptions = [
  { value: ContaStatus.Pendente, label: "Pendente", badge: "pendente" },
  { value: ContaStatus.Pago, label: "Recebido", badge: "ativo" },
  { value: ContaStatus.Vencido, label: "Vencido", badge: "vencido" },
  { value: ContaStatus.Cancelado, label: "Cancelado", badge: "cancelado" },
];

const statusStringMap: Record<string, ContaStatus> = {
  pendente: ContaStatus.Pendente,
  pago: ContaStatus.Pago,
  recebido: ContaStatus.Pago,
  vencido: ContaStatus.Vencido,
  cancelado: ContaStatus.Cancelado,
};

const knownStatusValues = statusOptions.map((option) => option.value);

const statusLookup = statusOptions.reduce<Record<number, { label: string; badge: string }>>((acc, option) => {
  acc[option.value] = option;
  return acc;
}, {});

const normalizeStatusValue = (status: ContaStatus | number | string): ContaStatus => {
  if (typeof status === "number" && knownStatusValues.includes(status as ContaStatus)) {
    return status as ContaStatus;
  }
  if (typeof status === "string") {
    const trimmed = status.trim();
    if (!trimmed) {
      return ContaStatus.Pendente;
    }
    const parsed = Number(trimmed);
    if (!Number.isNaN(parsed)) {
      return normalizeStatusValue(parsed as ContaStatus);
    }
    const mapped = statusStringMap[trimmed.toLowerCase()];
    if (mapped !== undefined) {
      return mapped;
    }
  }
  return ContaStatus.Pendente;
};

const resolveStatusMeta = (status: ContaStatus | number | string) => {
  const normalized = normalizeStatusValue(status);
  return statusLookup[normalized] ?? { label: "Desconhecido", badge: "pendente" };
};

const formaPagamentoOptions = [
  { value: FormaPagamento.Dinheiro, label: "Dinheiro" },
  { value: FormaPagamento.Pix, label: "PIX" },
  { value: FormaPagamento.CartaoCredito, label: "Cartão de crédito" },
  { value: FormaPagamento.CartaoDebito, label: "Cartão de débito" },
  { value: FormaPagamento.Transferencia, label: "Transferência" },
  { value: FormaPagamento.Boleto, label: "Boleto" },
  { value: FormaPagamento.Cheque, label: "Cheque" },
];

const formaPagamentoStringMap: Record<string, FormaPagamento> = {
  dinheiro: FormaPagamento.Dinheiro,
  pix: FormaPagamento.Pix,
  "cartão de crédito": FormaPagamento.CartaoCredito,
  "cartao credito": FormaPagamento.CartaoCredito,
  cartao_credito: FormaPagamento.CartaoCredito,
  cartaocredito: FormaPagamento.CartaoCredito,
  "cartão de débito": FormaPagamento.CartaoDebito,
  "cartao debito": FormaPagamento.CartaoDebito,
  cartao_debito: FormaPagamento.CartaoDebito,
  cartaodebito: FormaPagamento.CartaoDebito,
  cartao: FormaPagamento.CartaoDebito,
  transferencia: FormaPagamento.Transferencia,
  transferência: FormaPagamento.Transferencia,
  boleto: FormaPagamento.Boleto,
  cheque: FormaPagamento.Cheque,
};

const knownFormaPagamentoValues = formaPagamentoOptions.map((option) => option.value);

const normalizeFormaPagamentoValue = (value: FormaPagamento | number | string): FormaPagamento => {
  if (typeof value === "number" && knownFormaPagamentoValues.includes(value as FormaPagamento)) {
    return value as FormaPagamento;
  }
  if (typeof value === "string") {
    const trimmed = value.trim();
    if (!trimmed) {
      return FormaPagamento.Pix;
    }
    const parsed = Number(trimmed);
    if (!Number.isNaN(parsed)) {
      return normalizeFormaPagamentoValue(parsed as FormaPagamento);
    }
    const mapped = formaPagamentoStringMap[trimmed.toLowerCase()];
    if (mapped !== undefined) {
      return mapped;
    }
  }
  return FormaPagamento.Pix;
};

const currencyFormatter = new Intl.NumberFormat("pt-BR", {
  style: "currency",
  currency: "BRL",
});

const formatCurrency = (value: number | undefined | null) => currencyFormatter.format(value ?? 0);

const createEmptyItem = (): ContaFinanceiroItemForm => ({
  descricao: "",
  valor: undefined,
});

const createEmptyForm = (): ContaReceberFormState => ({
  clienteId: 0,
  descricao: "",
  valor: 0,
  dataVencimento: new Date().toISOString().substring(0, 10),
  dataRecebimento: undefined,
  status: ContaStatus.Pendente,
  formaPagamento: FormaPagamento.Pix,
  numeroParcelas: 1,
  intervaloDias: 30,
  itens: [createEmptyItem()],
});

const normalizeContaDto = (conta: ContaReceberDto): ContaReceberDto => ({
  ...conta,
  status: normalizeStatusValue(conta.status),
  formaPagamento: normalizeFormaPagamentoValue(conta.formaPagamento),
  parcelas: (conta.parcelas ?? []).map((parcela) => ({
    ...parcela,
    status: normalizeStatusValue(parcela.status),
  })),
});

const ContasReceberPage = ({ embedded = false }: { embedded?: boolean }) => {
  const { selectedEmpresaId, user } = useAuth();
  const [contas, setContas] = useState<ContaReceberDto[]>([]);
  const [clientes, setClientes] = useState<ClienteDto[]>([]);
  const [form, setForm] = useState<ContaReceberFormState>(createEmptyForm());
  const [statusFilter, setStatusFilter] = useState<ContaStatus | "all">("all");
  const [search, setSearch] = useState("");
  const [modalOpen, setModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [toast, setToast] = useState<string | null>(null);
  const [viewMode, setViewMode] = useState<"table" | "calendar">("table");
  const [currentMonth, setCurrentMonth] = useState(() => new Date(new Date().getFullYear(), new Date().getMonth(), 1));
  const [updatingStatusId, setUpdatingStatusId] = useState<number | null>(null);

  useEffect(() => {
    if (modalOpen) {
      document.body.classList.add("modal-open");
    } else {
      document.body.classList.remove("modal-open");
    }
    return () => document.body.classList.remove("modal-open");
  }, [modalOpen]);

  const normalizedRole = user?.role?.toLowerCase();
  const isGlobalAdmin = normalizedRole === "admin" && !user?.empresaId;

  useEffect(() => {
    const params = { empresaId: selectedEmpresaId ?? undefined };
    ContasReceberService.getAll(params)
      .then((data) => setContas(data.map((conta) => normalizeContaDto(conta))))
      .catch(() => setToast("Não foi possível carregar as contas a receber."));
  }, [selectedEmpresaId]);

  useEffect(() => {
    ClienteService.getAll(selectedEmpresaId)
      .then(setClientes)
      .catch(() => setToast("Não foi possível carregar os clientes."));
  }, [selectedEmpresaId]);

  const contaLookup = useMemo(() => {
    const map: Record<number, ContaReceberDto> = {};
    contas.forEach((conta) => {
      map[conta.id] = conta;
    });
    return map;
  }, [contas]);

  const filtered = useMemo(() => {
    return contas.filter((conta) => {
      if (statusFilter !== "all") {
        const contaStatus = normalizeStatusValue(conta.status);
        const parcelas = conta.parcelas ?? [];
        const hasParcelaWithStatus = parcelas.some(
          (parcela) => normalizeStatusValue(parcela.status ?? contaStatus) === statusFilter
        );
        if (contaStatus !== statusFilter && !hasParcelaWithStatus) {
          return false;
        }
      }
      if (search.trim()) {
        const term = search.trim().toLowerCase();
        if (!conta.descricao.toLowerCase().includes(term) && !conta.clienteNome.toLowerCase().includes(term)) {
          return false;
        }
      }
      return true;
    });
  }, [contas, statusFilter, search]);

  const totalItensValor = useMemo(() => {
    return (form.itens ?? []).reduce((total, item) => total + Number(item.valor ?? 0), 0);
  }, [form.itens]);

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    if (isGlobalAdmin) {
      setToast("Administradores globais possuem acesso somente leitura.");
      return;
    }

    const isEditing = Boolean(editingId);
    const { itens: formItens, numeroParcelas, intervaloDias, ...restForm } = form;

    let itensPayload: ContaFinanceiroItemInput[] | undefined;
    let valorFinal = restForm.valor;

    const itensNormalizados = (formItens ?? []).map((item) => ({
      descricao: item.descricao?.trim() ?? "",
      valor: Number(item.valor ?? 0),
    }));
    const itensValidos =
      isEditing && editingId
        ? []
        : itensNormalizados.filter((item) => item.descricao && item.descricao.trim().length > 0 && (item.valor ?? 0) > 0);

    const totalItens = itensValidos.reduce((total, item) => total + (item.valor ?? 0), 0);
    valorFinal = totalItens > 0 ? totalItens : restForm.valor;
    itensPayload = !isEditing && itensValidos.length ? itensValidos : undefined;

    const parcelasDefinidas = !isEditing && numeroParcelas && numeroParcelas > 1 ? numeroParcelas : undefined;
    const intervaloDefinido = parcelasDefinidas ? Math.max(1, intervaloDias ?? 30) : undefined;

    const payload: CreateContaReceberRequest = {
      ...restForm,
      valor: valorFinal,
      itens: itensPayload,
      numeroParcelas: parcelasDefinidas,
      intervaloDias: intervaloDefinido,
    };

    try {
      if (editingId) {
        const updated = await ContasReceberService.update(editingId, { ...payload, id: editingId });
        const normalized = normalizeContaDto(updated);
        setContas((prev) => prev.map((conta) => (conta.id === normalized.id ? normalized : conta)));
      } else {
        const created = await ContasReceberService.create(payload);
        const normalized = normalizeContaDto(created);
        setContas((prev) => [normalized, ...prev]);
      }
      setForm(createEmptyForm());
      setEditingId(null);
      setModalOpen(false);
      setToast("Conta salva com sucesso.");
    } catch (error: any) {
      setToast(getApiErrorMessage(error, "Não foi possível salvar a conta."));
    }
  };

  const handleOpenCreate = () => {
    if (isGlobalAdmin) {
      setToast("Administradores globais possuem acesso somente leitura.");
      return;
    }
    setForm({ ...createEmptyForm(), clienteId: clientes[0]?.id ?? 0 });
    setEditingId(null);
    setModalOpen(true);
  };

  const handleOpenEdit = (conta: ContaReceberDto) => {
    if (isGlobalAdmin) {
      setToast("Administradores globais possuem acesso somente leitura.");
      return;
    }
    setEditingId(conta.id);
    const nextForm: ContaReceberFormState = {
      clienteId: conta.clienteId,
      descricao: conta.descricao,
      valor: conta.valor,
      dataVencimento: conta.dataVencimento.substring(0, 10),
      dataRecebimento: conta.dataRecebimento ? conta.dataRecebimento.substring(0, 10) : undefined,
      status: normalizeStatusValue(conta.status),
      formaPagamento: normalizeFormaPagamentoValue(conta.formaPagamento),
      numeroParcelas: conta.totalParcelas || 1,
      intervaloDias: conta.intervaloDias || 30,
      itens:
        conta.itens && conta.itens.length
          ? conta.itens.map((item) => ({
              descricao: item.descricao,
              valor: item.valor,
            }))
          : [createEmptyItem()],
    };
    setForm(nextForm);
    setModalOpen(true);
  };

  const handleDelete = async (id: number) => {
    if (isGlobalAdmin) {
      setToast("Administradores globais possuem acesso somente leitura.");
      return;
    }
    if (!window.confirm("Deseja remover esta conta?")) {
      return;
    }
    await ContasReceberService.remove(id);
    setContas((prev) => prev.filter((conta) => conta.id !== id));
  };

  const itensReadOnly = Boolean(editingId);

  const handleItemChange = (index: number, field: "descricao" | "valor", value: string | number | undefined) => {
    if (itensReadOnly) {
      return;
    }
    setForm((prev) => {
      const itens = prev.itens ?? [createEmptyItem()];
      const current = itens[index];
      if (!current) {
        return prev;
      }
      const nextItens = itens.map((item, idx) => (idx === index ? { ...item, [field]: value } : item));
      return { ...prev, itens: nextItens };
    });
  };
  const handleServiceValueInput = (index: number, rawValue: string) => {
    if (itensReadOnly) {
      return;
    }
    if (rawValue === "") {
      setForm((prev) => {
        const itens = prev.itens ?? [createEmptyItem()];
        const nextItens = itens.map((item, idx) => (idx === index ? { ...item, valor: undefined } : item));
        return { ...prev, itens: nextItens };
      });
      return;
    }
    const numericValue = Number(rawValue);
    handleItemChange(index, "valor", Number.isNaN(numericValue) ? undefined : numericValue);
  };

  const addItem = () => {
    if (itensReadOnly) {
      return;
    }
    setForm((prev) => {
      const itens = prev.itens ?? [];
      return { ...prev, itens: [...itens, createEmptyItem()] };
    });
  };

  const removeItem = (index: number) => {
    if (itensReadOnly) {
      return;
    }
    setForm((prev) => {
      const itens = prev.itens ?? [];
      if (itens.length <= 1) return prev;
      return { ...prev, itens: itens.filter((_, idx) => idx !== index) };
    });
  };

  const parcelaEntries = useMemo<ParcelEntry[]>(() => {
    const entries: ParcelEntry[] = [];
    filtered.forEach((conta) => {
      const parcelas = conta.parcelas?.length
        ? conta.parcelas
        : [
            {
              id: 0,
              numero: 1,
              valor: conta.valor,
              dataVencimento: conta.dataVencimento,
              status: normalizeStatusValue(conta.status),
            },
          ];
      parcelas.forEach((parcela) => {
        const date = parcela.dataVencimento ? new Date(parcela.dataVencimento) : new Date(conta.dataVencimento);
        const statusValue = normalizeStatusValue(parcela.status ?? conta.status);
        entries.push({
          key: `${conta.id}-${parcela.id ?? parcela.numero}`,
          contaId: conta.id,
          parcelaId: parcela.id,
          contaDescricao: conta.descricao,
          clienteNome: conta.clienteNome,
          parcelaNumero: parcela.numero,
          totalParcelas: conta.totalParcelas || parcelas.length,
          valor: parcela.valor,
          status: statusValue,
          date,
        });
      });
    });
    return entries;
  }, [filtered]);

  const parcelasFiltradasMes = useMemo(() => {
    return parcelaEntries
      .filter(
        (entry) =>
          entry.date.getFullYear() === currentMonth.getFullYear() && entry.date.getMonth() === currentMonth.getMonth()
      )
      .sort((a, b) => a.date.getTime() - b.date.getTime() || a.parcelaNumero - b.parcelaNumero);
  }, [parcelaEntries, currentMonth]);

  const parcelaMap = useMemo(() => {
    const map: Record<string, ParcelEntry[]> = {};
    parcelaEntries.forEach((entry) => {
      const key = formatDateKey(entry.date);
      if (!map[key]) {
        map[key] = [];
      }
      map[key].push(entry);
    });
    return map;
  }, [parcelaEntries]);

  const todayKey = formatDateKey(new Date());

  const calendarDays = useMemo(() => {
    const startOfMonth = new Date(currentMonth.getFullYear(), currentMonth.getMonth(), 1);
    const startWeekday = startOfMonth.getDay();
    const firstCellDate = new Date(startOfMonth);
    firstCellDate.setDate(startOfMonth.getDate() - startWeekday);
    const days: Array<{
      key: string;
      date: Date;
      isCurrentMonth: boolean;
      isToday: boolean;
      parcelas: Array<{
        contaId: number;
        contaDescricao: string;
        clienteNome: string;
        parcelaNumero: number;
        valor: number;
        status: ContaStatus;
        parcelaId?: number;
      }>;
    }> = [];
    for (let i = 0; i < 42; i++) {
      const date = new Date(firstCellDate);
      date.setDate(firstCellDate.getDate() + i);
      const key = formatDateKey(date);
      days.push({
        key,
        date,
        isCurrentMonth: date.getMonth() === currentMonth.getMonth(),
        isToday: key === todayKey,
        parcelas: parcelaMap[key] ?? [],
      });
    }
    return days;
  }, [currentMonth, parcelaMap, todayKey]);

  const calendarSummary = useMemo(() => {
    let total = 0;
    let count = 0;
    parcelaEntries.forEach((entry) => {
      if (
        entry.date.getFullYear() === currentMonth.getFullYear() &&
        entry.date.getMonth() === currentMonth.getMonth()
      ) {
        total += entry.valor;
        count += 1;
      }
    });
    return { total, count };
  }, [parcelaEntries, currentMonth]);

  const handleChangeMonth = (delta: number) => {
    setCurrentMonth((prev) => new Date(prev.getFullYear(), prev.getMonth() + delta, 1));
  };

  const calendarMonthLabel = currentMonth.toLocaleDateString("pt-BR", {
    month: "long",
    year: "numeric",
  });

  const listSummary = useMemo(() => {
    let total = 0;
    let count = 0;
    parcelasFiltradasMes.forEach((entry) => {
      total += entry.valor;
      count += 1;
    });
    return { total, count };
  }, [parcelasFiltradasMes]);

  const buildUpdatePayload = (
    conta: ContaReceberDto,
    novoStatus: ContaStatus,
    dataRecebimento?: string | null
  ): UpdateContaReceberRequest => ({
    id: conta.id,
    clienteId: conta.clienteId,
    descricao: conta.descricao,
    valor: conta.valor,
    dataVencimento: conta.dataVencimento.substring(0, 10),
    dataRecebimento: dataRecebimento ?? (conta.dataRecebimento ? conta.dataRecebimento.substring(0, 10) : undefined),
    status: novoStatus,
    formaPagamento: conta.formaPagamento,
  });

  const handleStatusChange = async (contaId: number, parcelaId: number | undefined, novoStatus: ContaStatus) => {
    const conta = contaLookup[contaId];
    if (!conta) {
      setToast("Conta não encontrada.");
      return;
    }
    if (novoStatus === normalizeStatusValue(conta.status)) {
      setToast("Status já está atualizado.");
      return;
    }
    try {
      setUpdatingStatusId(contaId);
      const hasParcelaId = parcelaId !== undefined && parcelaId !== null;
      if (hasParcelaId) {
        const payloadParcela: UpdateContaReceberParcelaStatusRequest = {
          status: novoStatus,
          dataRecebimento: novoStatus === ContaStatus.Pago ? new Date().toISOString().substring(0, 10) : undefined,
        };
        const parcelaAtualizada = await ContasReceberService.updateParcelaStatus(contaId, parcelaId!, payloadParcela);
        setContas((prev) =>
          prev.map((c) => {
            if (c.id !== contaId) return c;
            const parcelasAtualizadas = (c.parcelas ?? []).map((p) =>
              p.id === parcelaAtualizada.id
                ? { ...p, status: parcelaAtualizada.status, dataRecebimento: parcelaAtualizada.dataRecebimento }
                : p
            );
            const todasPagas = parcelasAtualizadas.length > 0 && parcelasAtualizadas.every((p) => p.status === ContaStatus.Pago);
            return {
              ...c,
              status: todasPagas ? ContaStatus.Pago : c.status,
              dataRecebimento: todasPagas ? parcelaAtualizada.dataRecebimento ?? c.dataRecebimento : c.dataRecebimento,
              parcelas: parcelasAtualizadas,
            };
          })
        );
      } else {
        const payload = buildUpdatePayload(conta, novoStatus, novoStatus === ContaStatus.Pago ? new Date().toISOString().substring(0, 10) : conta.dataRecebimento);
        await ContasReceberService.update(contaId, payload);
        setContas((prev) =>
          prev.map((c) => {
            if (c.id !== contaId) {
              return c;
            }
            return {
              ...c,
              status: novoStatus,
              dataRecebimento: payload.dataRecebimento ?? c.dataRecebimento,
              parcelas: c.parcelas?.map((parcela) => ({ ...parcela, status: novoStatus })),
            };
          })
        );
      }
      setToast("Status atualizado com sucesso.");
    } catch (error: any) {
      setToast(getApiErrorMessage(error, "Não foi possível atualizar o status."));
    } finally {
      setUpdatingStatusId(null);
    }
  };

  const pageBody = (
    <>
      {toast && <div className="toast alert">{toast}</div>}
      {isGlobalAdmin && (
        <div className="toast info">Administradores globais possuem acesso somente leitura nesta tela.</div>
      )}

      <section className="filters-bar">
        <input
          className="search-input"
          placeholder="Buscar por descrição ou cliente"
          value={search}
          onChange={(event) => setSearch(event.target.value)}
        />
        <div className="select-wrapper">
          <select
            value={statusFilter}
            onChange={(event) =>
              setStatusFilter(event.target.value === "all" ? "all" : (Number(event.target.value) as ContaStatus))
            }
          >
            <option value="all">Todos</option>
            {statusOptions.map((status) => (
              <option key={status.value} value={status.value}>
                {status.label}
              </option>
            ))}
          </select>
        </div>
        <div className="filters-actions">
          <div className="view-toggle">
            <button
              type="button"
              className={`ghost ${viewMode === "table" ? "active" : ""}`}
              onClick={() => setViewMode("table")}
            >
              Lista
            </button>
            <button
              type="button"
              className={`ghost ${viewMode === "calendar" ? "active" : ""}`}
              onClick={() => setViewMode("calendar")}
            >
              Calendário
            </button>
          </div>
          {!isGlobalAdmin && (
            <button className="primary" onClick={handleOpenCreate}>
              + Nova conta
            </button>
          )}
        </div>
      </section>

      {viewMode === "table" ? (
        <section className="card table-wrapper">
          <div className="table-header">
            <div className="calendar-nav">
              <button type="button" className="ghost small" onClick={() => handleChangeMonth(-1)}>
                ‹
              </button>
              <h4>{calendarMonthLabel.charAt(0).toUpperCase() + calendarMonthLabel.slice(1)}</h4>
              <button type="button" className="ghost small" onClick={() => handleChangeMonth(1)}>
                ›
              </button>
            </div>
            <div className="calendar-summary">
              <span>{parcelasFiltradasMes.length} parcelas</span>
              <span>{formatCurrency(listSummary.total)}</span>
            </div>
          </div>
          <table>
            <thead>
              <tr>
                <th>Descrição</th>
                <th>Cliente</th>
                <th>Valor</th>
                <th>Vencimento</th>
                <th>Status</th>
                {!isGlobalAdmin && <th>Ações</th>}
              </tr>
            </thead>
            <tbody>
              {parcelasFiltradasMes.length === 0 ? (
                <tr>
                  <td colSpan={isGlobalAdmin ? 5 : 6}>Nenhuma conta neste mês.</td>
                </tr>
              ) : (
                parcelasFiltradasMes.map((entry) => {
                  const status = resolveStatusMeta(entry.status);
                  return (
                    <tr key={entry.key}>
                      <td className="table-title">
                        {entry.contaDescricao}
                        <small>Parcela #{entry.parcelaNumero}</small>
                      </td>
                      <td>{entry.clienteNome}</td>
                      <td>{entry.valor.toLocaleString("pt-BR", { style: "currency", currency: "BRL" })}</td>
                      <td>{entry.date.toLocaleDateString("pt-BR")}</td>
                      <td>
                        <span className={`badge ${status?.badge ?? "pendente"}`}>{status?.label ?? entry.status}</span>
                      </td>
                      {!isGlobalAdmin && (
                        <td>
                          <div className="table-actions">
                            <div className="status-update">
                              <div className="select-wrapper">
                                <select
                                  value={normalizeStatusValue(entry.status)}
                                  onChange={(event) =>
                                    handleStatusChange(
                                      entry.contaId,
                                      entry.parcelaId,
                                      Number(event.target.value) as ContaStatus
                                    )
                                  }
                                  disabled={updatingStatusId === entry.contaId}
                                >
                                  {statusOptions.map((option) => (
                                    <option key={option.value} value={option.value}>
                                      {option.label}
                                    </option>
                                  ))}
                                </select>
                              </div>
                            </div>
                            <button
                              className="ghost icon-only danger"
                              onClick={() => handleDelete(entry.contaId)}
                              aria-label="Remover conta"
                              title="Remover"
                            >
                              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                                <path
                                  d="M6.75 7.5H17.25M10 10.5V16.5M14 10.5V16.5M9 7.5L9.5 5.5C9.64 4.96 10.11 4.5 10.69 4.5H13.31C13.89 4.5 14.36 4.96 14.5 5.5L15 7.5M18 7.5L17.3 18.09C17.26 18.73 16.73 19.25 16.09 19.25H7.91C7.27 19.25 6.74 18.73 6.7 18.09L6 7.5H18Z"
                                  stroke="currentColor"
                                  strokeWidth="1.6"
                                  strokeLinecap="round"
                                  strokeLinejoin="round"
                                />
                              </svg>
                            </button>
                          </div>
                        </td>
                      )}
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </section>
      ) : (
        <section className="card calendar-wrapper">
          <header className="calendar-header">
            <div className="calendar-nav">
              <button type="button" className="ghost small" onClick={() => handleChangeMonth(-1)}>
                ‹
              </button>
              <h4>{calendarMonthLabel.charAt(0).toUpperCase() + calendarMonthLabel.slice(1)}</h4>
              <button type="button" className="ghost small" onClick={() => handleChangeMonth(1)}>
                ›
              </button>
            </div>
            <div className="calendar-summary">
              <span>{calendarSummary.count} parcelas</span>
              <span>{formatCurrency(calendarSummary.total)}</span>
            </div>
          </header>
      <div className="calendar-grid calendar-weekdays">
        {weekdayLabels.map((weekday) => (
          <div key={weekday} className="calendar-weekday">
            {weekday}
          </div>
        ))}
      </div>
      <div className="calendar-grid">
        {calendarDays.map((day) => {
          const totalsByStatus = day.parcelas.reduce<Record<number, { count: number; amount: number }>>((acc, parcela) => {
            const status = normalizeStatusValue(parcela.status);
            const current = acc[status] ?? { count: 0, amount: 0 };
            current.count += 1;
            current.amount += parcela.valor;
            acc[status] = current;
            return acc;
          }, {});
          const statusTotals = statusOptions
            .map((option) => {
              const totals = totalsByStatus[option.value];
              if (!totals) return null;
              const statusClass =
                option.value === ContaStatus.Cancelado
                  ? "cancelado"
                  : option.value === ContaStatus.Vencido
                  ? "vencido"
                  : option.badge;
              return { ...totals, label: option.label, className: statusClass };
            })
            .filter((item): item is { count: number; amount: number; label: string; className: string } => Boolean(item));
          const totalCount = day.parcelas.length;
          return (
            <div
              key={day.key}
              className={`calendar-cell ${day.isCurrentMonth ? "" : "faded"} ${day.isToday ? "today" : ""}`}
            >
              <div className="calendar-cell-header">
                <div className="calendar-day-number">{day.date.getDate()}</div>
              </div>
              {totalCount === 0 ? (
                <span className="calendar-empty">Sem parcelas</span>
              ) : (
                <div className="calendar-status-summary">
                  {statusTotals.map((status) => (
                    <div key={`${day.key}-${status.label}`} className="calendar-status-row">
                      <span className="calendar-status-left">
                        <span className="calendar-status-main">
                          <span className={`calendar-status-dot ${status.className}`} />
                          <span className="calendar-status-label">{status.label}</span>
                        </span>
                        <span className="calendar-status-count">{status.count}x</span>
                      </span>
                      <span className="calendar-status-amount">{formatCurrency(status.amount)}</span>
                    </div>
                  ))}
                </div>
              )}
            </div>
          );
        })}
      </div>
    </section>
  )}

      {!isGlobalAdmin && modalOpen && (
        <div className="modal-overlay" onClick={() => setModalOpen(false)}>
          <div className="modal" onClick={(event) => event.stopPropagation()}>
            <header className="modal-header">
              <h3>{editingId ? "Editar" : "Nova"} conta</h3>
              <button className="ghost" onClick={() => setModalOpen(false)}>
                Fechar
              </button>
            </header>
            <div className="modal-content">
              <form className="form-grid" onSubmit={handleSubmit}>
                <section className="form-section">
                  <div className="form-row">
                    <label className="form-field">
                      <span className="form-label">Cliente</span>
                      <div className="select-wrapper">
                        <select value={form.clienteId} onChange={(event) => setForm({ ...form, clienteId: Number(event.target.value) })}>
                          {clientes.map((cliente) => (
                            <option key={cliente.id} value={cliente.id}>
                              {cliente.nome}
                            </option>
                          ))}
                        </select>
                      </div>
                    </label>
                    <label className="form-field">
                      <span className="form-label">Forma de pagamento</span>
                      <div className="select-wrapper">
                        <select
                          value={form.formaPagamento}
                          onChange={(event) =>
                            setForm({ ...form, formaPagamento: Number(event.target.value) as FormaPagamento })
                          }
                        >
                          {formaPagamentoOptions.map((option) => (
                            <option key={option.value} value={option.value}>
                              {option.label}
                            </option>
                          ))}
                        </select>
                      </div>
                    </label>
                  </div>
                  <div className="form-row">
                    <label className="form-field">
                      <span className="form-label">Descrição</span>
                      <input value={form.descricao} onChange={(event) => setForm({ ...form, descricao: event.target.value })} required />
                    </label>
                  </div>
                  <div className="form-row">
                    <label className="form-field">
                      <span className="form-label">Vencimento</span>
                      <input type="date" value={form.dataVencimento} onChange={(event) => setForm({ ...form, dataVencimento: event.target.value })} required />
                    </label>
                    <label className="form-field">
                      <span className="form-label">Recebimento</span>
                      <input type="date" value={form.dataRecebimento ?? ""} onChange={(event) => setForm({ ...form, dataRecebimento: event.target.value || undefined })} />
                    </label>
                    <label className="form-field">
                      <span className="form-label">Status</span>
                      <div className="select-wrapper">
                        <select value={form.status} onChange={(event) => setForm({ ...form, status: Number(event.target.value) as ContaStatus })}>
                          {statusOptions.map((status) => (
                            <option key={status.value} value={status.value}>
                              {status.label}
                            </option>
                          ))}
                        </select>
                      </div>
                    </label>
                  </div>
                </section>

                <section className="form-section">
                  <div className="section-header">
                    <p className="form-section-title">Parcelamento</p>
                    <span className="input-hint">Defina parcelas se quiser dividir o valor em vezes.</span>
                  </div>
                  <div className="form-row">
                    <label className="form-field">
                      <span className="form-label">Quantidade de parcelas</span>
                      <input
                        type="number"
                        min={1}
                        value={form.numeroParcelas ?? ""}
                        onChange={(event) => {
                          const raw = event.target.value;
                          setForm((prev) => ({
                            ...prev,
                            numeroParcelas: raw === "" ? undefined : Math.max(1, Number(raw) || 1),
                          }));
                        }}
                        disabled={itensReadOnly}
                      />
                    </label>
                    <label className="form-field">
                      <span className="form-label">Intervalo (dias)</span>
                      <input
                        type="number"
                        min={1}
                        value={form.intervaloDias ?? ""}
                        onChange={(event) => {
                          const raw = event.target.value;
                          setForm((prev) => ({
                            ...prev,
                            intervaloDias: raw === "" ? undefined : Math.max(1, Number(raw) || 1),
                          }));
                        }}
                        disabled={itensReadOnly || (form.numeroParcelas ?? 1) <= 1}
                      />
                    </label>
                  </div>
                </section>

                <section className="form-section">
                  <div className="section-header">
                    <div>
                      <p className="form-section-title">Itens / Serviços</p>
                      <span className="input-hint">
                        Total calculado: {totalItensValor.toLocaleString("pt-BR", { style: "currency", currency: "BRL" })}
                      </span>
                      {itensReadOnly && (
                        <span className="input-hint">Itens existentes não podem ser alterados após a geração das parcelas.</span>
                      )}
                    </div>
                    <button type="button" className="ghost" onClick={addItem} disabled={itensReadOnly}>
                      + Adicionar item
                    </button>
                  </div>
                  <div className="modules-grid">
                    {(form.itens ?? []).map((item, index) => {
                      const valorDisplay = item.valor ?? "";
                      return (
                        <article key={index} className="module-card">
                          <header className="module-card-header">
                            <p>Item #{index + 1}</p>
                            <button
                              type="button"
                              className="ghost icon-only danger"
                              onClick={() => removeItem(index)}
                              disabled={itensReadOnly || (form.itens ?? []).length === 1}
                              aria-label="Remover item"
                              title="Remover item"
                            >
                              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                                <path
                                  d="M6.75 7.5H17.25M10 10.5V16.5M14 10.5V16.5M9 7.5L9.5 5.5C9.64 4.96 10.11 4.5 10.69 4.5H13.31C13.89 4.5 14.36 4.96 14.5 5.5L15 7.5M18 7.5L17.3 18.09C17.26 18.73 16.73 19.25 16.09 19.25H7.91C7.27 19.25 6.74 18.73 6.7 18.09L6 7.5H18Z"
                                  stroke="currentColor"
                                  strokeWidth="1.6"
                                  strokeLinecap="round"
                                  strokeLinejoin="round"
                                />
                              </svg>
                            </button>
                          </header>
                          <label className="form-field">
                            <span className="form-label">Descrição</span>
                            <input
                              placeholder="Descreva o serviço"
                              value={item.descricao}
                              onChange={(event) => handleItemChange(index, "descricao", event.target.value)}
                              readOnly={itensReadOnly}
                            />
                          </label>
                          <label className="form-field">
                            <span className="form-label">Valor</span>
                            <input
                              type="number"
                              min="0"
                              step="0.01"
                              value={valorDisplay}
                              onChange={(event) => handleServiceValueInput(index, event.target.value)}
                              readOnly={itensReadOnly}
                            />
                          </label>
                        </article>
                      );
                    })}
                  </div>
                </section>

                <div className="modal-actions">
                  <button type="button" className="ghost" onClick={() => setModalOpen(false)}>
                    Cancelar
                  </button>
                  <button type="submit" className="primary">
                    {editingId ? "Atualizar" : "Salvar"}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}
    </>
  );

  if (embedded) {
    return pageBody;
  }

  return (
    <AppLayout title="Contas a receber" subtitle="Acompanhe receitas e cobranças">
      {pageBody}
    </AppLayout>
  );
};

export default ContasReceberPage;

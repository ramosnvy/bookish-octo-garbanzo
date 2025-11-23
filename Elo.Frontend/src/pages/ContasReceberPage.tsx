import { useEffect, useMemo, useState } from "react";
import AppLayout from "../components/AppLayout";
import {
  ContaReceberDto,
  CreateContaReceberRequest,
  UpdateContaReceberRequest,
  ContaStatus,
  FormaPagamento,
  ClienteDto,
  ProdutoDto,
  ProdutoModuloDto,
  ContaFinanceiroItemInput,
} from "../models";
import { ContasReceberService } from "../services/FinanceiroService";
import { ClienteService } from "../services/ClienteService";
import { ProdutoService } from "../services/ProdutoService";
import { useAuth } from "../context/AuthContext";
import { getApiErrorMessage } from "../utils/apiError";
import {
  RECURRENCE_OPTIONS,
  RecurrencePresetValue,
  DEFAULT_RECURRENCE_INTERVAL,
  resolvePresetForInterval,
} from "../utils/recurrence";

const weekdayLabels = ["Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb"];

const formatDateKey = (date: Date) =>
  `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, "0")}-${String(date.getDate()).padStart(2, "0")}`;

type ContaFinanceiroItemForm = Omit<ContaFinanceiroItemInput, "valor"> & {
  valor?: number;
  tipo: "produto" | "servico";
};

type ContaReceberFormState = Omit<CreateContaReceberRequest, "itens"> & {
  itens: ContaFinanceiroItemForm[];
};

type ParcelEntry = {
  key: string;
  contaId: number;
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
  { value: ContaStatus.Vencido, label: "Vencido", badge: "inativo" },
  { value: ContaStatus.Cancelado, label: "Cancelado", badge: "inativo" },
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
  produtoId: undefined,
  produtoModuloIds: [],
  tipo: "servico",
});

const createEmptyForm = (): ContaReceberFormState => ({
  clienteId: 0,
  descricao: "",
  valor: 0,
  dataVencimento: new Date().toISOString().substring(0, 10),
  dataRecebimento: undefined,
  status: ContaStatus.Pendente,
  formaPagamento: FormaPagamento.Pix,
  isRecorrente: false,
  numeroParcelas: undefined,
  intervaloDias: undefined,
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

const ContasReceberPage = () => {
  const { selectedEmpresaId, user } = useAuth();
  const [contas, setContas] = useState<ContaReceberDto[]>([]);
  const [clientes, setClientes] = useState<ClienteDto[]>([]);
  const [produtos, setProdutos] = useState<ProdutoDto[]>([]);
  const [modulosPorProduto, setModulosPorProduto] = useState<Record<number, ProdutoModuloDto[]>>({});
  const [form, setForm] = useState<ContaReceberFormState>(createEmptyForm());
  const [statusFilter, setStatusFilter] = useState<ContaStatus | "all">("all");
  const [search, setSearch] = useState("");
  const [modalOpen, setModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [toast, setToast] = useState<string | null>(null);
  const [recurrencePreset, setRecurrencePreset] = useState<RecurrencePresetValue>(DEFAULT_RECURRENCE_INTERVAL);
  const [customInterval, setCustomInterval] = useState<number | null>(DEFAULT_RECURRENCE_INTERVAL);
  const [viewMode, setViewMode] = useState<"table" | "calendar">("table");
  const [currentMonth, setCurrentMonth] = useState(() => new Date(new Date().getFullYear(), new Date().getMonth(), 1));
  const [updatingStatusId, setUpdatingStatusId] = useState<number | null>(null);

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

  useEffect(() => {
    ProdutoService.getAll(selectedEmpresaId)
      .then((data) => {
        setProdutos(data);
        const map = data.reduce<Record<number, ProdutoModuloDto[]>>((acc, produto) => {
          acc[produto.id] = produto.modulos ?? [];
          return acc;
        }, {});
        setModulosPorProduto(map);
      })
      .catch(() => setToast("Não foi possível carregar os produtos."));
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
      if (statusFilter !== "all" && normalizeStatusValue(conta.status) !== statusFilter) {
        return false;
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

  const findProdutoById = (produtoId?: number) => {
    if (!produtoId) return undefined;
    return produtos.find((produto) => produto.id === produtoId);
  };

  const calculateProdutoValue = (item: ContaFinanceiroItemForm): number => {
    if (item.tipo !== "produto" || !item.produtoId) {
      return 0;
    }
    const produto = findProdutoById(item.produtoId);
    const base = produto?.valorRevenda ?? 0;
    const extras = (item.produtoModuloIds ?? []).reduce((total, moduloId) => {
      const modulo = modulosPorProduto[item.produtoId ?? 0]?.find((m) => m.id === moduloId);
      return total + (modulo?.valorAdicional ?? 0);
    }, 0);
    return Number((base + extras).toFixed(2));
  };

  const resolveItemDescricao = (item: ContaFinanceiroItemForm): string => {
    if (item.descricao?.trim()) {
      return item.descricao.trim();
    }
    if (item.tipo === "produto" && item.produtoId) {
      return findProdutoById(item.produtoId)?.nome ?? "";
    }
    return "";
  };

  const totalItensValor = useMemo(() => {
    return (form.itens ?? []).reduce((total, item) => {
      const valorItem = item.tipo === "produto" ? calculateProdutoValue(item) : Number(item.valor ?? 0);
      return total + valorItem;
    }, 0);
  }, [form.itens, produtos, modulosPorProduto]);

  const getCurrentIntervalValue = () =>
    recurrencePreset === "custom"
      ? (customInterval ?? DEFAULT_RECURRENCE_INTERVAL)
      : (recurrencePreset as number);

  const handleRecurrenceToggle = (checked: boolean) => {
    const intervalValue = getCurrentIntervalValue();
    setForm((prev) => ({
      ...prev,
      isRecorrente: checked,
      numeroParcelas: checked ? prev.numeroParcelas ?? 2 : undefined,
      intervaloDias: checked ? intervalValue : undefined,
    }));
  };

  const handleRecurrencePresetChange = (value: RecurrencePresetValue) => {
    setRecurrencePreset(value);
    if (value === "custom") {
      setForm((prev) => ({
        ...prev,
        intervaloDias: prev.isRecorrente ? customInterval ?? DEFAULT_RECURRENCE_INTERVAL : prev.intervaloDias,
      }));
    } else {
      setCustomInterval(DEFAULT_RECURRENCE_INTERVAL);
      setForm((prev) => ({
        ...prev,
        intervaloDias: prev.isRecorrente ? (value as number) : prev.intervaloDias,
      }));
    }
  };

  const handleCustomIntervalChange = (rawValue: string) => {
    if (rawValue === "") {
      setCustomInterval(null);
      setForm((prev) => ({
        ...prev,
        intervaloDias: prev.isRecorrente ? undefined : prev.intervaloDias,
      }));
      return;
    }
    const numericValue = Math.max(1, Number(rawValue) || 1);
    setCustomInterval(numericValue);
    setForm((prev) => ({
      ...prev,
      intervaloDias: prev.isRecorrente ? numericValue : prev.intervaloDias,
    }));
  };

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

    if (!isEditing) {
      const itensNormalizados = (formItens ?? []).map((item) => {
        const valorCalculado = item.tipo === "produto" ? calculateProdutoValue(item) : Number(item.valor ?? 0);
        const descricao = resolveItemDescricao(item);
        const payloadItem: ContaFinanceiroItemInput = {
          descricao,
          valor: valorCalculado,
          produtoId: item.tipo === "produto" ? item.produtoId : undefined,
          produtoModuloIds: item.tipo === "produto" ? item.produtoModuloIds : undefined,
        };
        return payloadItem;
      });

      const itensValidos = itensNormalizados.filter(
        (item) =>
          (item.descricao && item.descricao.trim().length > 0) ||
          (item.valor ?? 0) > 0 ||
          !!item.produtoId ||
          ((item.produtoModuloIds?.length ?? 0) > 0)
      );

      const totalItens = itensValidos.reduce((total, item) => total + (item.valor ?? 0), 0);
      valorFinal = totalItens > 0 ? totalItens : restForm.valor;
      itensPayload = itensValidos.length ? itensValidos : undefined;
    }
    const payload: CreateContaReceberRequest = {
      ...restForm,
      valor: valorFinal,
      itens: itensPayload,
      numeroParcelas: !isEditing && restForm.isRecorrente ? numeroParcelas : undefined,
      intervaloDias: !isEditing && restForm.isRecorrente ? intervaloDias : undefined,
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
      setRecurrencePreset(DEFAULT_RECURRENCE_INTERVAL);
      setCustomInterval(DEFAULT_RECURRENCE_INTERVAL);
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
    setRecurrencePreset(DEFAULT_RECURRENCE_INTERVAL);
    setCustomInterval(DEFAULT_RECURRENCE_INTERVAL);
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
      isRecorrente: conta.isRecorrente,
      numeroParcelas: conta.isRecorrente ? conta.totalParcelas : undefined,
      intervaloDias: conta.isRecorrente ? conta.intervaloDias : undefined,
      itens:
        conta.itens && conta.itens.length
          ? conta.itens.map((item) => ({
              descricao: item.descricao,
              valor: item.produtoId ? 0 : item.valor,
              produtoId: item.produtoId,
              produtoModuloIds:
                item.produtoModuloIds && item.produtoModuloIds.length
                  ? item.produtoModuloIds
                  : item.produtoModuloId
                  ? [item.produtoModuloId]
                  : [],
              tipo: item.produtoId ? "produto" : "servico",
            }))
          : [createEmptyItem()],
    };
    setForm(nextForm);
    const preset = resolvePresetForInterval(conta.isRecorrente ? conta.intervaloDias : undefined);
    setRecurrencePreset(preset);
    if (preset === "custom") {
      setCustomInterval(conta.intervaloDias ?? DEFAULT_RECURRENCE_INTERVAL);
    } else {
      setCustomInterval(DEFAULT_RECURRENCE_INTERVAL);
    }
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
  const recurrenceDisabled = Boolean(editingId);

  const handleItemChange = (
    index: number,
    field: keyof ContaFinanceiroItemInput,
    value: string | number | number[] | undefined
  ) => {
    if (itensReadOnly) {
      return;
    }
    setForm((prev) => {
      const itens = prev.itens ?? [createEmptyItem()];
      const current = itens[index];
      if (!current) {
        return prev;
      }
      if (field === "valor" && current.tipo === "produto") {
        return prev;
      }
      const nextItens = itens.map((item, idx) => (idx === index ? { ...item, [field]: value } : item));
      return { ...prev, itens: nextItens };
    });
  };

  const handleProdutoChange = (index: number, produtoId?: number) => {
    if (itensReadOnly) {
      return;
    }
    setForm((prev) => {
      const itens = prev.itens ?? [createEmptyItem()];
      const nextItens = itens.map((item, idx) => {
        if (idx !== index) return item;
        const produto = findProdutoById(produtoId);
        const descricao = item.descricao?.trim().length ? item.descricao : produto?.nome ?? "";
        return {
          ...item,
          produtoId,
          produtoModuloIds: [],
          descricao,
        };
      });
      return { ...prev, itens: nextItens };
    });
  };

  const handleModuloSelectionChange = (index: number, selected: number[]) => {
    if (itensReadOnly) {
      return;
    }
    handleItemChange(index, "produtoModuloIds", selected);
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

  const handleItemTypeChange = (index: number, tipo: "produto" | "servico") => {
    if (itensReadOnly) {
      return;
    }
    setForm((prev) => {
      const itens = prev.itens ?? [createEmptyItem()];
      const nextItens = itens.map((item, idx) => {
        if (idx !== index) return item;
        if (item.tipo === tipo) return item;
        if (tipo === "produto") {
          return {
            ...item,
            tipo,
            produtoId: undefined,
            produtoModuloIds: [],
            valor: undefined,
          };
        }
        return {
          ...item,
          tipo,
          produtoId: undefined,
          produtoModuloIds: [],
          valor: item.valor ?? undefined,
        };
      });
      return { ...prev, itens: nextItens };
    });
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
    calendarDays.forEach((day) => {
      if (!day.isCurrentMonth) {
        return;
      }
      day.parcelas.forEach((parcela) => {
        total += parcela.valor;
        count += 1;
      });
    });
    return { total, count };
  }, [calendarDays]);

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
    isRecorrente: conta.isRecorrente,
  });

  const handleStatusChange = async (contaId: number, novoStatus: ContaStatus) => {
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
      setToast("Status atualizado com sucesso.");
    } catch (error: any) {
      setToast(getApiErrorMessage(error, "Não foi possível atualizar o status."));
    } finally {
      setUpdatingStatusId(null);
    }
  };

  return (
    <AppLayout title="Contas a receber" subtitle="Acompanhe receitas e cobranças">
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
                                <label className="select-wrapper status-select">
                                  <span className={`badge ${status?.badge ?? "pendente"}`}>{status?.label ?? "Status"}</span>
                                  <select
                                    value={normalizeStatusValue(entry.status)}
                                    onChange={(event) =>
                                      handleStatusChange(entry.contaId, Number(event.target.value) as ContaStatus)
                                    }
                                    disabled={updatingStatusId === entry.contaId}
                                  >
                                    {statusOptions.map((option) => (
                                      <option key={option.value} value={option.value}>
                                        {option.label}
                                      </option>
                                    ))}
                                  </select>
                                </label>
                              </div>
                              <button className="ghost" onClick={() => handleDelete(entry.contaId)}>
                                Remover
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
              const displayed = day.parcelas.slice(0, 3);
              const remaining = day.parcelas.length - displayed.length;
              return (
                <div
                  key={day.key}
                  className={`calendar-cell ${day.isCurrentMonth ? "" : "faded"} ${day.isToday ? "today" : ""}`}
                >
                  <div className="calendar-day-number">{day.date.getDate()}</div>
                  <div className="calendar-events">
                    {displayed.length === 0 ? (
                      <span className="calendar-empty">Sem parcelas</span>
                    ) : (
                      <>
                        {displayed.map((parcela, index) => {
                  const status = resolveStatusMeta(parcela.status);
                          return (
                            <div
                              key={`${day.key}-${parcela.contaId}-${parcela.parcelaNumero}-${index}`}
                              className="calendar-event"
                            >
                              <p className="calendar-event-title">{parcela.contaDescricao}</p>
                              <small>
                                Parcela #{parcela.parcelaNumero} • {formatCurrency(parcela.valor)}
                              </small>
                              <span className={`badge ${status?.badge ?? "pendente"}`}>
                                {status?.label ?? parcela.status}
                              </span>
                              {!isGlobalAdmin && (
                                <div className="calendar-event-actions">
                                  <span className={`badge ${status?.badge ?? "pendente"}`}>{status?.label ?? "Status"}</span>
                                  <select
                                    value={parcela.status}
                                    onChange={(event) =>
                                      handleStatusChange(parcela.contaId, Number(event.target.value) as ContaStatus)
                                    }
                                    disabled={updatingStatusId === parcela.contaId}
                                  >
                                    {statusOptions.map((option) => (
                                      <option key={option.value} value={option.value}>
                                        {option.label}
                                      </option>
                                    ))}
                                  </select>
                                </div>
                              )}
                            </div>
                          );
                        })}
                        {remaining > 0 && <span className="calendar-more">+{remaining} mais</span>}
                      </>
                    )}
                  </div>
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
                    <p className="form-section-title">Recorrência</p>
                  </div>
                  <div className="switch-field">
                    <div>
                      <span className="form-label">Conta recorrente</span>
                      <p className="input-hint">Gera parcelas futuras seguindo o intervalo configurado.</p>
                    </div>
                    <label className="toggle-switch">
                      <input
                        type="checkbox"
                        checked={form.isRecorrente ?? false}
                        onChange={(event) => handleRecurrenceToggle(event.target.checked)}
                        disabled={recurrenceDisabled}
                      />
                      <span className="slider" />
                    </label>
                  </div>
                  {form.isRecorrente && (
                    <>
                      <div className="form-row">
                        <label className="form-field">
                          <span className="form-label">Quantidade de repetições</span>
                          <input
                            type="number"
                            min={1}
                            value={form.numeroParcelas !== undefined ? form.numeroParcelas : ""}
                            onChange={(event) => {
                              const raw = event.target.value;
                              setForm((prev) => ({
                                ...prev,
                                numeroParcelas:
                                  raw === "" ? undefined : Math.max(1, Number(raw) || 1),
                              }));
                            }}
                            disabled={recurrenceDisabled}
                          />
                        </label>
                      </div>
                      <div className="form-row">
                        <label className="form-field">
                          <span className="form-label">Periodicidade</span>
                          <div className="select-wrapper">
                            <select
                              value={recurrencePreset === "custom" ? "custom" : String(recurrencePreset)}
                              onChange={(event) => {
                                const raw = event.target.value;
                                const presetValue = raw === "custom" ? "custom" : Number(raw);
                                handleRecurrencePresetChange(presetValue as RecurrencePresetValue);
                              }}
                              disabled={recurrenceDisabled}
                            >
                              {RECURRENCE_OPTIONS.map((option) => (
                                <option
                                  key={option.label}
                                  value={option.value === "custom" ? "custom" : String(option.value)}
                                >
                                  {option.label}
                                </option>
                              ))}
                            </select>
                          </div>
                        </label>
                        {recurrencePreset === "custom" && (
                          <label className="form-field">
                            <span className="form-label">Intervalo em dias</span>
                            <input
                              type="number"
                              min={1}
                              value={customInterval ?? ""}
                              onChange={(event) => handleCustomIntervalChange(event.target.value)}
                              disabled={recurrenceDisabled}
                            />
                          </label>
                        )}
                      </div>
                    </>
                  )}
                </section>

                <section className="form-section">
                  <div className="section-header">
                    <div>
                      <p className="form-section-title">Produtos, serviços ou módulos</p>
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
                      const valorCalculado = item.tipo === "produto" ? calculateProdutoValue(item) : Number(item.valor ?? 0);
                      const isProduto = item.tipo === "produto";
                      const valorDisplay = isProduto ? valorCalculado : item.valor ?? "";
                      return (
                        <article key={index} className="module-card">
                          <header className="module-card-header">
                            <p>Item #{index + 1}</p>
                            <button
                              type="button"
                              className="ghost small"
                              onClick={() => removeItem(index)}
                              disabled={itensReadOnly || (form.itens ?? []).length === 1}
                            >
                              Remover
                            </button>
                          </header>
                          <div className="form-row">
                            <label className="form-field">
                              <span className="form-label">Tipo do item</span>
                              <div className="select-wrapper">
                                <select
                                  value={item.tipo}
                                  onChange={(event) =>
                                    handleItemTypeChange(index, event.target.value as "produto" | "servico")
                                  }
                                  disabled={itensReadOnly}
                                >
                                  <option value="produto">Produto cadastrado</option>
                                  <option value="servico">Serviço manual</option>
                                </select>
                              </div>
                            </label>
                          </div>
                          {item.tipo === "produto" && (
                            <>
                              <div className="form-row">
                                <label className="form-field">
                                  <span className="form-label">Produto</span>
                                  <div className="select-wrapper">
                                    <select
                                      value={item.produtoId ?? ""}
                                      onChange={(event) =>
                                        handleProdutoChange(
                                          index,
                                          event.target.value ? Number(event.target.value) : undefined
                                        )
                                      }
                                      disabled={itensReadOnly || !produtos.length}
                                    >
                                      <option value="">Selecione</option>
                                      {produtos.map((produto) => (
                                        <option key={produto.id} value={produto.id}>
                                          {produto.nome} • {formatCurrency(produto.valorRevenda)}
                                        </option>
                                      ))}
                                    </select>
                                  </div>
                                </label>
                                <label className="form-field">
                                  <span className="form-label">Módulos (opcionais)</span>
                                  <div className="select-wrapper multi-select">
                                    <select
                                      multiple
                                      value={(item.produtoModuloIds ?? []).map(String)}
                                      onChange={(event) =>
                                        handleModuloSelectionChange(
                                          index,
                                          Array.from(event.target.selectedOptions, (option) => Number(option.value))
                                        )
                                      }
                                      disabled={itensReadOnly || !item.produtoId}
                                    >
                                      {(modulosPorProduto[item.produtoId ?? 0] ?? []).map((modulo) => (
                                        <option key={modulo.id} value={modulo.id}>
                                          {modulo.nome} (+{formatCurrency(modulo.valorAdicional)})
                                        </option>
                                      ))}
                                    </select>
                                  </div>
                                </label>
                              </div>
                            </>
                          )}
                          <label className="form-field">
                            <span className="form-label">Descrição</span>
                            <input
                              placeholder="Descreva o serviço ou item"
                              value={item.descricao}
                              onChange={(event) => handleItemChange(index, "descricao", event.target.value)}
                              readOnly={itensReadOnly}
                            />
                          </label>
                          <label className="form-field">
                            <span className="form-label">
                              {isProduto ? "Valor calculado" : "Valor"}
                            </span>
                            <input
                              type="number"
                              min="0"
                              step="0.01"
                              value={valorDisplay}
                              onChange={(event) => handleServiceValueInput(index, event.target.value)}
                              readOnly={isProduto || itensReadOnly}
                            />
                            {isProduto && (
                              <span className="input-hint">Valor do produto somado aos módulos selecionados.</span>
                            )}
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
    </AppLayout>
  );
};

export default ContasReceberPage;

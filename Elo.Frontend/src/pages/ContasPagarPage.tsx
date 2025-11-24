import { useEffect, useMemo, useState } from "react";
import AppLayout from "../components/AppLayout";
import {
  ContaPagarDto,
  CreateContaPagarRequest,
  UpdateContaPagarRequest,
  ContaStatus,
  FornecedorDto,
} from "../models";
import { ContasPagarService } from "../services/FinanceiroService";
import { FornecedorService } from "../services/FornecedorService";
import { useAuth } from "../context/AuthContext";
import { getApiErrorMessage } from "../utils/apiError";

const weekdayLabels = ["Dom", "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb"];

const formatDateKey = (date: Date) =>
  `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, "0")}-${String(date.getDate()).padStart(2, "0")}`;

const statusOptions = [
  { value: ContaStatus.Pendente, label: "Pendente", badge: "pendente" },
  { value: ContaStatus.Pago, label: "Pago", badge: "ativo" },
  { value: ContaStatus.Vencido, label: "Vencido", badge: "vencido" },
  { value: ContaStatus.Cancelado, label: "Cancelado", badge: "cancelado" },
];

const statusLookup = statusOptions.reduce<Record<number, { label: string; badge: string }>>((acc, option) => {
  acc[option.value] = option;
  return acc;
}, {});

const statusStringMap: Record<string, ContaStatus> = {
  pendente: ContaStatus.Pendente,
  pago: ContaStatus.Pago,
  vencido: ContaStatus.Vencido,
  cancelado: ContaStatus.Cancelado,
};

const normalizeStatusValue = (status: ContaStatus | number | string): ContaStatus => {
  if (typeof status === "number" && statusLookup[status]) return status as ContaStatus;
  if (typeof status === "string") {
    const trimmed = status.trim().toLowerCase();
    if (trimmed in statusStringMap) return statusStringMap[trimmed];
    const parsed = Number(trimmed);
    if (!Number.isNaN(parsed) && statusLookup[parsed]) return parsed as ContaStatus;
  }
  return ContaStatus.Pendente;
};

const createEmptyForm = (): CreateContaPagarRequest => ({
  fornecedorId: 0,
  descricao: "",
  valor: 0,
  dataVencimento: new Date().toISOString().substring(0, 10),
  dataPagamento: undefined,
  status: ContaStatus.Pendente,
  categoria: "",
  numeroParcelas: 1,
  intervaloDias: undefined,
});

type PayableCalendarEntry = {
  key: string;
  contaId: number;
  contaDescricao: string;
  fornecedorNome: string;
  valor: number;
  status: ContaStatus;
  parcelaNumero: number;
  date: Date;
};

type CalendarCell = {
  key: string;
  date: Date;
  isCurrentMonth: boolean;
  isToday: boolean;
  parcelas: PayableCalendarEntry[];
};

const formatCurrency = (value: number) =>
  value.toLocaleString("pt-BR", { style: "currency", currency: "BRL" });

const ContasPagarPage = ({ embedded = false }: { embedded?: boolean }) => {
  const { selectedEmpresaId, user } = useAuth();
  const [contas, setContas] = useState<ContaPagarDto[]>([]);
  const [form, setForm] = useState<CreateContaPagarRequest>(createEmptyForm());
  const [fornecedores, setFornecedores] = useState<FornecedorDto[]>([]);
  const [statusFilter, setStatusFilter] = useState<ContaStatus | "all">("all");
  const [search, setSearch] = useState("");
  const [modalOpen, setModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [toast, setToast] = useState<string | null>(null);
  const [updatingStatusId, setUpdatingStatusId] = useState<number | null>(null);
  const [viewMode, setViewMode] = useState<"table" | "calendar">("table");
  const [currentMonth, setCurrentMonth] = useState(
    () => new Date(new Date().getFullYear(), new Date().getMonth(), 1)
  );
  const normalizedRole = user?.role?.toLowerCase();
  const isGlobalAdmin = normalizedRole === "admin" && !user?.empresaId;

  useEffect(() => {
    const params = { empresaId: selectedEmpresaId ?? undefined };
    ContasPagarService.getAll(params)
      .then(setContas)
      .catch(() => setToast("Não foi possível carregar as contas a pagar."));
  }, [selectedEmpresaId]);

  useEffect(() => {
    FornecedorService.getAll(selectedEmpresaId)
      .then(setFornecedores)
      .catch(() => setToast("Não foi possível carregar os fornecedores."));
  }, [selectedEmpresaId]);

  const parcelEntries = useMemo<PayableCalendarEntry[]>(() => {
    const entries: PayableCalendarEntry[] = [];
    contas.forEach((conta) => {
      const parcelas =
        conta.parcelas && conta.parcelas.length
          ? conta.parcelas
          : [
              {
                id: 0,
                numero: 1,
                valor: conta.valor,
                dataVencimento: conta.dataVencimento,
                status: conta.status,
              },
            ];
      parcelas.forEach((parcela) => {
        const date = new Date(parcela.dataVencimento ?? conta.dataVencimento);
        entries.push({
          key: `${conta.id}-${parcela.id ?? parcela.numero}`,
          contaId: conta.id,
          contaDescricao: conta.descricao,
          fornecedorNome: conta.fornecedorNome,
          valor: parcela.valor ?? conta.valor,
          status: parcela.status ?? conta.status,
          parcelaNumero: parcela.numero ?? 1,
          date,
        });
      });
    });
    return entries;
  }, [contas]);

  const filteredParcelEntries = useMemo(() => {
    return parcelEntries.filter((entry) => {
      if (statusFilter !== "all" && entry.status !== statusFilter) {
        return false;
      }
      if (search.trim()) {
        const term = search.trim().toLowerCase();
        if (!entry.contaDescricao.toLowerCase().includes(term) && !entry.fornecedorNome.toLowerCase().includes(term)) {
          return false;
        }
      }
      return true;
    });
  }, [parcelEntries, statusFilter, search]);

  const entriesByDate = useMemo(() => {
    const map: Record<string, PayableCalendarEntry[]> = {};
    filteredParcelEntries.forEach((entry) => {
      const key = formatDateKey(entry.date);
      map[key] = map[key] ? [...map[key], entry] : [entry];
    });
    return map;
  }, [filteredParcelEntries]);

  const calendarCells = useMemo<CalendarCell[]>(() => {
    const firstDay = new Date(currentMonth.getFullYear(), currentMonth.getMonth(), 1);
    const start = new Date(firstDay);
    start.setDate(start.getDate() - start.getDay());
    const todayKey = formatDateKey(new Date());
    const cells: CalendarCell[] = [];
    for (let i = 0; i < 42; i += 1) {
      const cellDate = new Date(start);
      cellDate.setDate(start.getDate() + i);
      const key = formatDateKey(cellDate);
      cells.push({
        key,
        date: cellDate,
        isCurrentMonth: cellDate.getMonth() === currentMonth.getMonth(),
        isToday: key === todayKey,
        parcelas: entriesByDate[key] ?? [],
      });
    }
    return cells;
  }, [entriesByDate, currentMonth]);

  const currentMonthEntries = useMemo(() => {
    return filteredParcelEntries.filter(
      (entry) =>
        entry.date.getMonth() === currentMonth.getMonth() && entry.date.getFullYear() === currentMonth.getFullYear()
    );
  }, [filteredParcelEntries, currentMonth]);

  const calendarTotals = useMemo(() => {
    return currentMonthEntries.reduce(
      (acc, entry) => {
        acc.count += 1;
        acc.total += entry.valor;
        return acc;
      },
      { count: 0, total: 0 }
    );
  }, [currentMonthEntries]);

  const handleMonthChange = (offset: number) => {
    setCurrentMonth((prev) => new Date(prev.getFullYear(), prev.getMonth() + offset, 1));
  };

  const currentMonthLabel = currentMonth.toLocaleDateString("pt-BR", { month: "long", year: "numeric" });

  const handleStatusChange = async (contaId: number, novoStatus: ContaStatus) => {
    if (isGlobalAdmin) {
      setToast("Administradores globais possuem acesso somente leitura.");
      return;
    }
    const conta = contas.find((c) => c.id === contaId);
    if (!conta) {
      setToast("Conta não encontrada.");
      return;
    }
    if (normalizeStatusValue(conta.status) === novoStatus) {
      setToast("Status já está atualizado.");
      return;
    }
    try {
      setUpdatingStatusId(contaId);
      const payload: UpdateContaPagarRequest = {
        id: conta.id,
        fornecedorId: conta.fornecedorId,
        descricao: conta.descricao,
        valor: conta.valor,
        dataVencimento: conta.dataVencimento.substring(0, 10),
        dataPagamento: conta.dataPagamento ? conta.dataPagamento.substring(0, 10) : undefined,
        status: novoStatus,
        categoria: conta.categoria,
        numeroParcelas: conta.totalParcelas || 1,
        intervaloDias: conta.intervaloDias ?? undefined,
      };
      const updated = await ContasPagarService.update(contaId, payload);
      setContas((prev) => prev.map((c) => (c.id === updated.id ? updated : c)));
      setToast("Status atualizado.");
    } catch (error: any) {
      setToast(getApiErrorMessage(error, "Não foi possível atualizar o status."));
    } finally {
      setUpdatingStatusId(null);
    }
  };

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    if (isGlobalAdmin) {
      setToast("Administradores globais possuem acesso somente leitura.");
      return;
    }
    const { numeroParcelas, intervaloDias, ...rest } = form;
    const parcelasDefinidas = !editingId && numeroParcelas && numeroParcelas > 1 ? numeroParcelas : undefined;
    const intervaloDefinido = parcelasDefinidas ? Math.max(1, intervaloDias ?? 30) : undefined;
    const payload: CreateContaPagarRequest = {
      ...rest,
      numeroParcelas: parcelasDefinidas,
      intervaloDias: intervaloDefinido,
    };
    try {
      if (editingId) {
        const updated = await ContasPagarService.update(editingId, { ...payload, id: editingId });
        setContas((prev) => prev.map((conta) => (conta.id === updated.id ? updated : conta)));
      } else {
        const created = await ContasPagarService.create(payload);
        setContas((prev) => [created, ...prev]);
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
    const freshForm = createEmptyForm();
    freshForm.fornecedorId = fornecedores[0]?.id ?? 0;
    freshForm.categoria = "Manual";
    setForm(freshForm);
    setEditingId(null);
    setModalOpen(true);
  };

  const handleOpenEdit = (conta: ContaPagarDto) => {
    if (isGlobalAdmin) {
      setToast("Administradores globais possuem acesso somente leitura.");
      return;
    }
    setEditingId(conta.id);
    setForm({
      fornecedorId: conta.fornecedorId,
      descricao: conta.descricao,
      valor: conta.valor,
      dataVencimento: conta.dataVencimento.substring(0, 10),
      dataPagamento: conta.dataPagamento ? conta.dataPagamento.substring(0, 10) : undefined,
      status: conta.status,
      categoria: conta.categoria,
      numeroParcelas: conta.totalParcelas || 1,
      intervaloDias: conta.intervaloDias ?? undefined,
    });
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
    await ContasPagarService.remove(id);
    setContas((prev) => prev.filter((conta) => conta.id !== id));
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
          placeholder="Buscar por descrição ou fornecedor"
          value={search}
          onChange={(event) => setSearch(event.target.value)}
        />
        <div className="filters-group compact">
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
              <button type="button" className="ghost small" onClick={() => handleMonthChange(-1)}>
                ‹
              </button>
              <h4>{currentMonth.toLocaleDateString("pt-BR", { month: "long", year: "numeric" })}</h4>
              <button type="button" className="ghost small" onClick={() => handleMonthChange(1)}>
                ›
              </button>
            </div>
            <div className="calendar-summary">
              <span>{currentMonthEntries.length} parcelas</span>
              <span>{formatCurrency(calendarTotals.total)}</span>
            </div>
          </div>
          <table>
            <thead>
              <tr>
                <th>Descrição</th>
                <th>Fornecedor</th>
                <th>Valor</th>
                <th>Vencimento</th>
                <th>Status</th>
                {!isGlobalAdmin && <th>Ações</th>}
              </tr>
            </thead>
            <tbody>
              {currentMonthEntries.length === 0 ? (
                <tr>
                  <td colSpan={isGlobalAdmin ? 5 : 6}>Nenhuma conta neste mês.</td>
                </tr>
              ) : (
                currentMonthEntries.map((entry) => {
                  const status = statusOptions.find((option) => option.value === entry.status);
                  const contaRef = contas.find((c) => c.id === entry.contaId);
                  return (
                    <tr key={entry.key}>
                      <td>
                        <p className="table-title">{entry.contaDescricao}</p>
                        <small>Parcela #{entry.parcelaNumero}</small>
                      </td>
                      <td>{entry.fornecedorNome}</td>
                      <td>{formatCurrency(entry.valor)}</td>
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
                                  value={entry.status}
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
                              </div>
                            </div>
                            <button
                              className="ghost icon-only danger"
                              onClick={() => handleDelete(entry.contaId)}
                              aria-label="Remover conta"
                              title="Remover conta"
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
              <button type="button" className="ghost small" onClick={() => handleMonthChange(-1)}>
                ‹
              </button>
              <h4>{currentMonthLabel.charAt(0).toUpperCase() + currentMonthLabel.slice(1)}</h4>
              <button type="button" className="ghost small" onClick={() => handleMonthChange(1)}>
                ›
              </button>
            </div>
            <div className="calendar-summary">
              <span>{calendarTotals.count} parcelas</span>
              <span>{formatCurrency(calendarTotals.total)}</span>
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
            {calendarCells.map((cell) => {
              const totalsByStatus = cell.parcelas.reduce<Record<number, { count: number; amount: number }>>((acc, parcela) => {
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
                  return { ...totals, label: option.label, className: option.badge };
                })
                .filter((item): item is { count: number; amount: number; label: string; className: string } => Boolean(item));
              const totalCount = cell.parcelas.length;
              return (
                <div
                  key={cell.key}
                  className={`calendar-cell ${cell.isCurrentMonth ? "" : "faded"} ${cell.isToday ? "today" : ""}`}
                >
                  <div className="calendar-cell-header">
                    <div className="calendar-day-number">{cell.date.getDate()}</div>
                  </div>
                  {totalCount === 0 ? (
                    <span className="calendar-empty">Sem contas</span>
                  ) : (
                    <div className="calendar-status-summary">
                      {statusTotals.map((status) => (
                        <div key={`${cell.key}-${status.label}`} className="calendar-status-row">
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
                <div className="form-row">
                  <label className="form-field">
                    <span className="form-label">Fornecedor</span>
                    <div className="select-wrapper">
                      <select value={form.fornecedorId} onChange={(event) => setForm({ ...form, fornecedorId: Number(event.target.value) })}>
                        {fornecedores.map((fornecedor) => (
                          <option key={fornecedor.id} value={fornecedor.id}>
                            {fornecedor.nome}
                          </option>
                        ))}
                      </select>
                    </div>
                  </label>
                  <label className="form-field">
                    <span className="form-label">Categoria</span>
                    <input value={form.categoria} onChange={(event) => setForm({ ...form, categoria: event.target.value })} />
                  </label>
                </div>
                <div className="form-row">
                  <label className="form-field">
                    <span className="form-label">Descrição</span>
                    <input value={form.descricao} onChange={(event) => setForm({ ...form, descricao: event.target.value })} required />
                  </label>
                  <label className="form-field">
                    <span className="form-label">Valor</span>
                    <input type="number" step="0.01" value={form.valor} onChange={(event) => setForm({ ...form, valor: Number(event.target.value) })} required />
                  </label>
                </div>
                <div className="form-row">
                  <label className="form-field">
                    <span className="form-label">Vencimento</span>
                    <input type="date" value={form.dataVencimento} onChange={(event) => setForm({ ...form, dataVencimento: event.target.value })} required />
                  </label>
                  <label className="form-field">
                    <span className="form-label">Pagamento</span>
                    <input type="date" value={form.dataPagamento ?? ""} onChange={(event) => setForm({ ...form, dataPagamento: event.target.value || undefined })} />
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
                <section className="form-section">
                  <div className="section-header">
                    <div>
                      <p className="form-section-title">Parcelamento</p>
                      <span className="input-hint">Divida o valor em parcelas, se necessário.</span>
                    </div>
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
                        disabled={Boolean(editingId)}
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
                        disabled={Boolean(editingId) || (form.numeroParcelas ?? 1) <= 1}
                      />
                    </label>
                  </div>
                </section>
                <div className="modal-actions">
                  <button type="button" className="ghost" onClick={() => setModalOpen(false)}>
                    Cancelar
                  </button>
                  <button type="submit" className="primary">
                    {editingId ? "Salvar" : "Adicionar"}
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
    <AppLayout title="Contas a pagar" subtitle="Gerencie obrigações financeiras">
      {pageBody}
    </AppLayout>
  );
};

export default ContasPagarPage;

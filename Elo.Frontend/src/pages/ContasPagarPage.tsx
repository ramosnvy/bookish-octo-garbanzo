import { useEffect, useMemo, useState } from "react";
import AppLayout from "../components/AppLayout";
import {
  ContaPagarDto,
  CreateContaPagarRequest,
  ContaStatus,
  FornecedorDto,
} from "../models";
import { ContasPagarService } from "../services/FinanceiroService";
import { FornecedorService } from "../services/FornecedorService";
import { useAuth } from "../context/AuthContext";
import {
  RECURRENCE_OPTIONS,
  RecurrencePresetValue,
  DEFAULT_RECURRENCE_INTERVAL,
  resolvePresetForInterval,
} from "../utils/recurrence";
import { getApiErrorMessage } from "../utils/apiError";

const statusOptions = [
  { value: ContaStatus.Pendente, label: "Pendente", badge: "pendente" },
  { value: ContaStatus.Pago, label: "Pago", badge: "ativo" },
  { value: ContaStatus.Vencido, label: "Vencido", badge: "inativo" },
  { value: ContaStatus.Cancelado, label: "Cancelado", badge: "inativo" },
];

const createEmptyForm = (): CreateContaPagarRequest => ({
  fornecedorId: 0,
  descricao: "",
  valor: 0,
  dataVencimento: new Date().toISOString().substring(0, 10),
  dataPagamento: undefined,
  status: ContaStatus.Pendente,
  categoria: "",
  isRecorrente: false,
  numeroParcelas: undefined,
  intervaloDias: undefined,
});

const ContasPagarPage = () => {
  const { selectedEmpresaId, user } = useAuth();
  const [contas, setContas] = useState<ContaPagarDto[]>([]);
  const [form, setForm] = useState<CreateContaPagarRequest>(createEmptyForm());
  const [fornecedores, setFornecedores] = useState<FornecedorDto[]>([]);
  const [statusFilter, setStatusFilter] = useState<ContaStatus | "all">("all");
  const [search, setSearch] = useState("");
  const [modalOpen, setModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [toast, setToast] = useState<string | null>(null);
  const [recurrencePreset, setRecurrencePreset] = useState<RecurrencePresetValue>(DEFAULT_RECURRENCE_INTERVAL);
  const [customInterval, setCustomInterval] = useState<number | null>(DEFAULT_RECURRENCE_INTERVAL);
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

  const filtered = useMemo(() => {
    return contas.filter((conta) => {
      if (statusFilter !== "all" && conta.status !== statusFilter) {
        return false;
      }
      if (search.trim()) {
        const term = search.trim().toLowerCase();
        if (!conta.descricao.toLowerCase().includes(term) && !conta.fornecedorNome.toLowerCase().includes(term)) {
          return false;
        }
      }
      return true;
    });
  }, [contas, statusFilter, search]);

  const recurrenceDisabled = Boolean(editingId);

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
    const { isRecorrente, numeroParcelas, intervaloDias, ...rest } = form;
    const payload: CreateContaPagarRequest = {
      ...rest,
      isRecorrente,
      numeroParcelas: !editingId && isRecorrente ? numeroParcelas : undefined,
      intervaloDias: !editingId && isRecorrente ? intervaloDias : undefined,
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
    const freshForm = createEmptyForm();
    freshForm.fornecedorId = fornecedores[0]?.id ?? 0;
    setForm(freshForm);
    setRecurrencePreset(DEFAULT_RECURRENCE_INTERVAL);
    setCustomInterval(DEFAULT_RECURRENCE_INTERVAL);
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
      isRecorrente: conta.isRecorrente,
      numeroParcelas: conta.isRecorrente ? conta.totalParcelas : undefined,
      intervaloDias: conta.isRecorrente ? conta.intervaloDias : undefined,
    });
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
    await ContasPagarService.remove(id);
    setContas((prev) => prev.filter((conta) => conta.id !== id));
  };

  return (
    <AppLayout title="Contas a pagar" subtitle="Gerencie obrigações financeiras">
      {toast && <div className="toast alert">{toast}</div>}
      {isGlobalAdmin && (
        <div className="toast info">Administradores globais possuem acesso somente leitura nesta tela.</div>
      )}
      <section className="filters-bar">
        <input className="search-input" placeholder="Buscar por descrição ou fornecedor" value={search} onChange={(event) => setSearch(event.target.value)} />
        <div className="select-wrapper">
          <select value={statusFilter} onChange={(event) => setStatusFilter(event.target.value === "all" ? "all" : (Number(event.target.value) as ContaStatus))}>
            <option value="all">Todos</option>
            {statusOptions.map((status) => (
              <option key={status.value} value={status.value}>
                {status.label}
              </option>
            ))}
          </select>
        </div>
        {!isGlobalAdmin && (
          <button className="primary" onClick={handleOpenCreate}>
            + Nova conta
          </button>
        )}
      </section>

      <section className="card table-wrapper">
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
            {filtered.length === 0 ? (
              <tr>
                <td colSpan={6}>Nenhuma conta encontrada.</td>
              </tr>
            ) : (
              filtered.map((conta) => {
                const status = statusOptions.find((option) => option.value === conta.status);
                return (
                  <tr key={conta.id}>
                    <td>
                      <p className="table-title">{conta.descricao}</p>
                      <small>Categoria: {conta.categoria || "-"}</small>
                    </td>
                    <td>{conta.fornecedorNome}</td>
                    <td>{conta.valor.toLocaleString("pt-BR", { style: "currency", currency: "BRL" })}</td>
                    <td>{new Date(conta.dataVencimento).toLocaleDateString("pt-BR")}</td>
                    <td>
                      <span className={`badge ${status?.badge ?? "pendente"}`}>{status?.label ?? conta.status}</span>
                    </td>
                    {!isGlobalAdmin && (
                      <td>
                        <div className="table-actions">
                          <button className="ghost" onClick={() => handleOpenEdit(conta)}>
                            Editar
                          </button>
                          <button className="ghost" onClick={() => handleDelete(conta.id)}>
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
                    <p className="form-section-title">Recorrência</p>
                  </div>
                  <div className="switch-field">
                    <div>
                      <span className="form-label">Conta recorrente</span>
                      <p className="input-hint">Divide o valor em várias parcelas com intervalo fixo.</p>
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
                          <span className="form-label">Quantidade de parcelas</span>
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
    </AppLayout>
  );
};

export default ContasPagarPage;

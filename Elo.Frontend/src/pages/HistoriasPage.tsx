import { useEffect, useMemo, useState } from "react";
import AppLayout from "../components/AppLayout";
import {
  ClienteDto,
  HistoriaDto,
  HistoriaMovimentacaoDto,
  HistoriaStatus,
  HistoriaTipo,
  UpdateHistoriaRequest,
  CreateHistoriaRequest,
  ProdutoDto,
} from "../models";
import { ClienteService } from "../services/ClienteService";
import { ProdutoService } from "../services/ProdutoService";
import { HistoriaService } from "../services/HistoriaService";
import { UserService } from "../services/UserService";
import { useAuth } from "../context/AuthContext";
import "../components.css";
import "../styles.css";

type FormState = {
  id?: number;
  clienteId: number;
  usuarioResponsavelId: number;
  status: HistoriaStatus;
  tipo: HistoriaTipo;
  dataInicio?: string | null;
  dataFinalizacao?: string | null;
  observacoes?: string | null;
  produtosSelecionados: number[];
  modulosSelecionados: Record<number, number[]>;
};

const statusOrder: HistoriaStatus[] = [
  HistoriaStatus.Pendente,
  HistoriaStatus.EmAndamento,
  HistoriaStatus.Pausada,
  HistoriaStatus.Concluida,
  HistoriaStatus.Cancelada,
];

const statusLabels: Record<HistoriaStatus, string> = {
  [HistoriaStatus.Pendente]: "Pendente",
  [HistoriaStatus.EmAndamento]: "Em andamento",
  [HistoriaStatus.Concluida]: "Concluída",
  [HistoriaStatus.Cancelada]: "Cancelada",
  [HistoriaStatus.Pausada]: "Pausada",
};

const tipoLabels: Record<HistoriaTipo, string> = {
  [HistoriaTipo.Projeto]: "Projeto",
  [HistoriaTipo.Entrega]: "Entrega",
  [HistoriaTipo.Operacao]: "Operação",
  [HistoriaTipo.Implementacao]: "Implementação",
  [HistoriaTipo.OrdemDeServico]: "Ordem de Serviço",
};

const badgeTone: Record<HistoriaStatus, string> = {
  [HistoriaStatus.Pendente]: "#f59e0b",
  [HistoriaStatus.EmAndamento]: "#3b82f6",
  [HistoriaStatus.Pausada]: "#8b5cf6",
  [HistoriaStatus.Concluida]: "#16a34a",
  [HistoriaStatus.Cancelada]: "#dc2626",
};

const HistoriasPage = () => {
  const { selectedEmpresaId } = useAuth();
  const [historias, setHistorias] = useState<HistoriaDto[]>([]);
  const [clientes, setClientes] = useState<ClienteDto[]>([]);
  const [produtos, setProdutos] = useState<ProdutoDto[]>([]);
  const [usuarios, setUsuarios] = useState<{ id: number; nome: string }[]>([]);
  const [produtoBusca, setProdutoBusca] = useState("");
  const [loading, setLoading] = useState(false);
  const [erroCarregamento, setErroCarregamento] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const [showModal, setShowModal] = useState(false);
  const [editing, setEditing] = useState<HistoriaDto | null>(null);
  const [form, setForm] = useState<FormState>({
    clienteId: 0,
    usuarioResponsavelId: 0,
    status: HistoriaStatus.Pendente,
    tipo: HistoriaTipo.Projeto,
    dataInicio: "",
    dataFinalizacao: "",
    observacoes: "",
    produtosSelecionados: [],
    modulosSelecionados: {},
  });
  const [filtroTipo, setFiltroTipo] = useState<HistoriaTipo | "todos">("todos");

  useEffect(() => {
    if (showModal) {
      document.body.classList.add("modal-open");
    } else {
      document.body.classList.remove("modal-open");
    }
    return () => document.body.classList.remove("modal-open");
  }, [showModal]);

  useEffect(() => {
    const fetchData = async () => {
      setLoading(true);
      setErroCarregamento(null);
      try {
        const [hist, cli, prod, usr] = await Promise.all([
          HistoriaService.getAll({ empresaId: selectedEmpresaId ?? undefined }),
          ClienteService.getAll(selectedEmpresaId ?? undefined),
          ProdutoService.getAll(selectedEmpresaId ?? undefined),
          UserService.getAll(selectedEmpresaId ?? undefined),
        ]);
        setHistorias(hist);
        setClientes(cli);
        setProdutos(prod);
        setUsuarios(usr.map((u) => ({ id: u.id, nome: u.nome })));
      } catch (error) {
        console.error("Erro ao carregar dados de histórias", error);
        setErroCarregamento("Não foi possível carregar clientes, usuários ou produtos. Verifique seu acesso/empresa.");
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, [selectedEmpresaId]);

  const resetForm = () => {
    setEditing(null);
    setForm({
      clienteId: 0,
      usuarioResponsavelId: 0,
      status: HistoriaStatus.Pendente,
      tipo: HistoriaTipo.Projeto,
      dataInicio: "",
      dataFinalizacao: "",
      observacoes: "",
      produtosSelecionados: [],
      modulosSelecionados: {},
    });
  };

  const openCreate = () => {
    resetForm();
    setShowModal(true);
  };

  const openEdit = (historia: HistoriaDto) => {
    setEditing(historia);
    setForm({
      id: historia.id,
      clienteId: historia.clienteId,
      usuarioResponsavelId: historia.usuarioResponsavelId,
      status: historia.status,
      tipo: historia.tipo,
      dataInicio: historia.dataInicio?.slice(0, 10),
      dataFinalizacao: historia.dataFinalizacao?.slice(0, 10) ?? "",
      observacoes: historia.observacoes ?? "",
      produtosSelecionados: [historia.produtoId],
      modulosSelecionados: {},
    });
    setShowModal(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.clienteId || form.produtosSelecionados.length === 0 || !form.usuarioResponsavelId) return;
    setSaving(true);
    try {
      const produtoId = form.produtosSelecionados[0]; // API atual aceita um produto por história
      const payload: CreateHistoriaRequest = {
        clienteId: form.clienteId,
        produtoId,
        usuarioResponsavelId: form.usuarioResponsavelId,
        status: form.status,
        tipo: form.tipo,
        dataInicio: form.dataInicio || null,
        dataFinalizacao: form.dataFinalizacao || null,
        observacoes: form.observacoes || null,
      };
      let result: HistoriaDto;
      if (form.id) {
        const updatePayload: UpdateHistoriaRequest = { ...payload, id: form.id };
        result = await HistoriaService.update(form.id, updatePayload);
      } else {
        result = await HistoriaService.create(payload);
      }
      setHistorias((prev) => {
        const exists = prev.some((h) => h.id === result.id);
        if (exists) {
          return prev.map((h) => (h.id === result.id ? result : h));
        }
        return [result, ...prev];
      });
      setShowModal(false);
      resetForm();
    } finally {
      setSaving(false);
    }
  };

  const handleStatusChange = async (historia: HistoriaDto, novoStatus: HistoriaStatus) => {
    if (historia.status === novoStatus) return;
    setSaving(true);
    try {
      const updated = await HistoriaService.adicionarMovimentacao(historia.id, { statusNovo: novoStatus });
      setHistorias((prev) => prev.map((h) => (h.id === updated.id ? updated : h)));
    } finally {
      setSaving(false);
    }
  };

  const handleDelete = async (id: number) => {
    if (!confirm("Deseja remover esta história?")) return;
    await HistoriaService.remove(id);
    setHistorias((prev) => prev.filter((h) => h.id !== id));
  };

  const historiasFiltradas = useMemo(() => {
    return historias.filter((h) => (filtroTipo === "todos" ? true : h.tipo === filtroTipo));
  }, [historias, filtroTipo]);

  const columns = useMemo(
    () =>
      statusOrder.map((status) => ({
        status,
        items: historiasFiltradas.filter((h) => h.status === status),
      })),
    [historiasFiltradas]
  );

  const produtosFiltrados = useMemo(() => {
    const termo = produtoBusca.trim().toLowerCase();
    if (!termo) return produtos;
    return produtos.filter((p) => p.nome.toLowerCase().includes(termo));
  }, [produtoBusca, produtos]);

  const renderCard = (historia: HistoriaDto) => {
    const ultimaMov: HistoriaMovimentacaoDto | undefined = historia.movimentacoes?.[0];
    const responsavel = historia.usuarioResponsavelNome || "Sem responsável";
    return (
      <div className="kanban-card">
        <div className="kanban-card-header">
          <div className="kanban-pill" style={{ backgroundColor: badgeTone[historia.status] }}>
            {statusLabels[historia.status]}
          </div>
          <button className="ghost small danger icon-only" onClick={() => handleDelete(historia.id)} title="Remover">
            ×
          </button>
        </div>
        <div className="kanban-title">{tipoLabels[historia.tipo]}</div>
        <div className="kanban-meta">
          <strong>Cliente:</strong> {historia.clienteNome}
        </div>
        <div className="kanban-meta">
          <strong>Produto:</strong> {historia.produtoNome}
        </div>
        <div className="kanban-meta">
          <strong>Responsável:</strong> {responsavel}
        </div>
        {historia.observacoes && <p className="kanban-notes">{historia.observacoes}</p>}
        <div className="kanban-meta subtle">
          <strong>Início:</strong> {historia.dataInicio?.slice(0, 10)}
          {historia.dataFinalizacao ? ` · Fim: ${historia.dataFinalizacao.slice(0, 10)}` : ""}
        </div>
        {ultimaMov && (
          <div className="kanban-meta subtle">
            <strong>Última mov.:</strong> {ultimaMov.dataMovimentacao.slice(0, 10)}
          </div>
        )}
        <div className="kanban-actions">
          <select
            value={historia.status}
            onChange={(e) => handleStatusChange(historia, Number(e.target.value) as HistoriaStatus)}
          >
            {statusOrder.map((s) => (
              <option key={s} value={s}>
                {statusLabels[s]}
              </option>
            ))}
          </select>
          <button className="ghost small" onClick={() => openEdit(historia)}>
            Editar
          </button>
        </div>
      </div>
    );
  };

  return (
    <AppLayout
      title="Histórias"
      subtitle="Acompanhe projetos, entregas e implementações em um quadro kanban."
      actions={
        <div className="actions-bar">
          <select value={filtroTipo} onChange={(e) => setFiltroTipo(e.target.value === "todos" ? "todos" : Number(e.target.value) as HistoriaTipo)}>
            <option value="todos">Todos os tipos</option>
            {Object.values(HistoriaTipo)
              .filter((v) => typeof v === "number")
              .map((v) => (
                <option key={v} value={v as number}>
                  {tipoLabels[v as HistoriaTipo]}
                </option>
              ))}
          </select>
          <button className="primary" onClick={openCreate}>
            Nova história
          </button>
        </div>
      }
    >
      {erroCarregamento && <div className="card danger">{erroCarregamento}</div>}
      {loading ? (
        <div className="card">Carregando histórias...</div>
      ) : (
        <div className="kanban-board">
          {columns.map((col) => (
            <div key={col.status} className="kanban-column">
              <div className="kanban-column-header">
                <span className="kanban-column-title">{statusLabels[col.status]}</span>
                <span className="kanban-count">{col.items.length}</span>
              </div>
              <div className="kanban-column-body">
                {col.items.length === 0 ? (
                  <div className="kanban-empty">Nenhuma história aqui.</div>
                ) : (
                  col.items.map((historia) => <div key={historia.id}>{renderCard(historia)}</div>)
                )}
              </div>
            </div>
          ))}
        </div>
      )}

      {showModal && (
        <div className="modal-overlay">
          <div className="modal historia-modal">
            <div className="modal-header">
              <div>
                <p className="modal-eyebrow">{editing ? "Editar história" : "Nova história"}</p>
                <h2>{editing ? "Atualizar" : "Cadastrar"} história</h2>
              </div>
              <button className="ghost icon-only" onClick={() => setShowModal(false)}>
                ×
              </button>
            </div>
            <div className="modal-content">
              <form className="form-grid" onSubmit={handleSubmit}>
                <div className="form-row">
                  <label className="form-field">
                    Cliente
                    <select
                      value={form.clienteId}
                      onChange={(e) => setForm((f) => ({ ...f, clienteId: Number(e.target.value) }))}
                      required
                    >
                      <option value={0}>Selecione</option>
                      {clientes.map((c) => (
                        <option key={c.id} value={c.id}>
                          {c.nome}
                        </option>
                      ))}
                    </select>
                  </label>
                  <label className="form-field form-field--full historia-field">
                    <div className="historia-field-head">
                      <div>
                        <div className="field-label-strong">Produtos (múltipla seleção)</div>
                        <p className="input-subtitle">Escolha um ou mais produtos para vincular a história</p>
                      </div>
                      <span className="pill-badge ghosty">Vincule produtos</span>
                    </div>
                    <div className="historia-panel">
                      <div className="product-filter">
                        <div className="product-search-wrapper">
                          <svg viewBox="0 0 20 20" aria-hidden="true">
                            <path
                              d="M14.5 13.5l3 3M9.5 15a5.5 5.5 0 1 1 0-11 5.5 5.5 0 0 1 0 11z"
                              fill="none"
                              stroke="currentColor"
                              strokeWidth="1.4"
                              strokeLinecap="round"
                            />
                          </svg>
                          <input
                            type="search"
                            className="product-search"
                            placeholder="Buscar produto por nome"
                            value={produtoBusca}
                            onChange={(e) => setProdutoBusca(e.target.value)}
                          />
                        </div>
                      </div>
                      <div className="product-list">
                        {produtosFiltrados.length === 0 ? (
                          <div className="product-empty">Nenhum produto encontrado para “{produtoBusca}”.</div>
                        ) : (
                          <div className="product-grid">
                            {produtosFiltrados.map((p) => {
                              const selecionado = form.produtosSelecionados.includes(p.id);
                              const modulosCount = p.modulos?.length ?? 0;
                              return (
                                <label key={p.id} className={`product-card ${selecionado ? "is-selected" : ""}`}>
                                  <input
                                    type="checkbox"
                                    checked={selecionado}
                                    onChange={(e) =>
                                      setForm((prev) => {
                                        const selected = new Set(prev.produtosSelecionados);
                                        if (e.target.checked) selected.add(p.id);
                                        else selected.delete(p.id);
                                        return { ...prev, produtosSelecionados: Array.from(selected) };
                                      })
                                    }
                                  />
                                  <div className="product-check">
                                    <span className="product-check-icon">✓</span>
                                  </div>
                                  <div className="product-card-body">
                                    <div className="product-card-top">
                                      <span className="product-name">{p.nome}</span>
                                      <span className="pill-badge">{selecionado ? "Selecionado" : "Disponível"}</span>
                                    </div>
                                    <div className="product-meta">
                                      <span className="product-chip strong">
                                        {modulosCount} módulo{modulosCount === 1 ? "" : "s"}
                                      </span>
                                      {p.fornecedorNome && <span className="product-chip alt">{p.fornecedorNome}</span>}
                                    </div>
                                  </div>
                                </label>
                              );
                            })}
                          </div>
                        )}
                      </div>
                    </div>
                  </label>
                </div>
                {form.produtosSelecionados.length > 0 && (
                  <div className="form-row">
                    <label className="form-field form-field--full">
                      Módulos dos produtos selecionados
                      <p className="input-subtitle">Selecione os módulos que farão parte desta implementação</p>
                      <div className="historia-panel muted">
                        <div className="module-groups">
                          {produtos
                            .filter((p) => form.produtosSelecionados.includes(p.id))
                            .map((p) => (
                              <div key={p.id} className="module-group">
                                <div className="module-group-title">{p.nome}</div>
                                <div className="chip-grid pill-grid">
                                  {(p.modulos ?? []).map((m) => (
                                    <label key={`${p.id}-${m.id}`} className="chip-option pill">
                                      <input
                                        type="checkbox"
                                        checked={form.modulosSelecionados[p.id]?.includes(m.id) || false}
                                        onChange={(e) =>
                                          setForm((prev) => {
                                            const current = prev.modulosSelecionados[p.id] || [];
                                            const nextSet = new Set(current);
                                            if (e.target.checked) nextSet.add(m.id);
                                            else nextSet.delete(m.id);
                                            return {
                                              ...prev,
                                              modulosSelecionados: {
                                                ...prev.modulosSelecionados,
                                                [p.id]: Array.from(nextSet),
                                              },
                                            };
                                          })
                                        }
                                      />
                                      <span>{m.nome}</span>
                                    </label>
                                  ))}
                                  {(p.modulos ?? []).length === 0 && <span className="input-hint">Nenhum módulo cadastrado para este produto.</span>}
                                </div>
                              </div>
                            ))}
                        </div>
                        <div className="historia-info-banner">A API atual aceita apenas um produto por história; será enviado o primeiro selecionado.</div>
                      </div>
                    </label>
                  </div>
                )}
                <div className="form-row">
                  <label className="form-field">
                    Responsável
                    <select
                      value={form.usuarioResponsavelId}
                      onChange={(e) => setForm((f) => ({ ...f, usuarioResponsavelId: Number(e.target.value) }))}
                      required
                    >
                      <option value={0}>Selecione</option>
                      {usuarios.map((u) => (
                        <option key={u.id} value={u.id}>
                          {u.nome}
                        </option>
                      ))}
                    </select>
                  </label>
                  <label className="form-field">
                    Tipo
                    <select
                      value={form.tipo}
                      onChange={(e) => setForm((f) => ({ ...f, tipo: Number(e.target.value) as HistoriaTipo }))}
                    >
                      {Object.values(HistoriaTipo)
                        .filter((v) => typeof v === "number")
                        .map((v) => (
                          <option key={v} value={v as number}>
                            {tipoLabels[v as HistoriaTipo]}
                          </option>
                        ))}
                    </select>
                  </label>
                  <label className="form-field">
                    Status
                    <select
                      value={form.status}
                      onChange={(e) => setForm((f) => ({ ...f, status: Number(e.target.value) as HistoriaStatus }))}
                    >
                      {statusOrder.map((s) => (
                        <option key={s} value={s}>
                          {statusLabels[s]}
                        </option>
                      ))}
                    </select>
                  </label>
                </div>
                <div className="form-row">
                  <label className="form-field">
                    Início
                    <input
                      type="date"
                      value={form.dataInicio ?? ""}
                      onChange={(e) => setForm((f) => ({ ...f, dataInicio: e.target.value }))}
                    />
                  </label>
                  <label className="form-field">
                    Finalização
                    <input
                      type="date"
                      value={form.dataFinalizacao ?? ""}
                      onChange={(e) => setForm((f) => ({ ...f, dataFinalizacao: e.target.value }))}
                    />
                  </label>
                </div>
                <label className="form-field form-field--full">
                  Observações
                  <textarea
                    value={form.observacoes ?? ""}
                    onChange={(e) => setForm((f) => ({ ...f, observacoes: e.target.value }))}
                    rows={3}
                    placeholder="Contexto, escopo ou detalhes importantes..."
                  />
                </label>
                <div className="form-actions">
                  <button type="button" className="ghost" onClick={() => setShowModal(false)}>
                    Cancelar
                  </button>
                  <button type="submit" className="primary" disabled={saving}>
                    {saving ? "Salvando..." : "Salvar"}
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

export default HistoriasPage;

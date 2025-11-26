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

const currencyFormatter = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });

const statusIcons: Record<HistoriaStatus, JSX.Element> = {
  [HistoriaStatus.Pendente]: (
    <svg viewBox="0 0 24 24" className="status-icon soft">
      <circle cx="12" cy="12" r="10" fill="none" stroke="currentColor" strokeWidth="2" />
      <path d="M12 6v6l4 2" stroke="currentColor" strokeWidth="2" strokeLinejoin="round" strokeLinecap="round" />
    </svg>
  ),
  [HistoriaStatus.EmAndamento]: (
    <svg viewBox="0 0 24 24" className="status-icon work">
      <path d="M4 12h16M8 6h8M8 18h8" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
      <circle cx="6" cy="12" r="2" fill="currentColor" />
    </svg>
  ),
  [HistoriaStatus.Pausada]: (
    <svg viewBox="0 0 24 24" className="status-icon pause">
      <path d="M10 6h2v12h-2zM14 6h2v12h-2z" fill="currentColor" />
    </svg>
  ),
  [HistoriaStatus.Concluida]: (
    <svg viewBox="0 0 24 24" className="status-icon done">
      <circle cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="2" fill="none" />
      <path d="M8 12l2 2 4-4" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  ),
  [HistoriaStatus.Cancelada]: (
    <svg viewBox="0 0 24 24" className="status-icon cancel">
      <circle cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="2" fill="none" />
      <path d="M9 9l6 6M15 9l-6 6" stroke="currentColor" strokeWidth="2" strokeLinecap="round" />
    </svg>
  ),
};

const HistoriasPage = () => {
  const { selectedEmpresaId } = useAuth();
  const empresaIdParam = selectedEmpresaId ?? undefined;
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
  const [dragOverStatus, setDragOverStatus] = useState<HistoriaStatus | null>(null);
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
    const produtosVinculados =
      historia.produtos && historia.produtos.length > 0
        ? historia.produtos
        : [
            {
              produtoId: historia.produtoId,
              produtoNome: historia.produtoNome,
              produtoModuloIds: [],
              produtoModuloNomes: [],
            },
          ];
    const modulosPreenchidos = produtosVinculados.reduce<Record<number, number[]>>((acc, produto) => {
      if (produto.produtoModuloIds && produto.produtoModuloIds.length > 0) {
        acc[produto.produtoId] = produto.produtoModuloIds;
      }
      return acc;
    }, {});
    setForm({
      id: historia.id,
      clienteId: historia.clienteId,
      usuarioResponsavelId: historia.usuarioResponsavelId,
      status: historia.status,
      tipo: historia.tipo,
      dataInicio: historia.dataInicio?.slice(0, 10),
      dataFinalizacao: historia.dataFinalizacao?.slice(0, 10) ?? "",
      observacoes: historia.observacoes ?? "",
      produtosSelecionados: produtosVinculados.map((p) => p.produtoId),
      modulosSelecionados: modulosPreenchidos,
    });
    setShowModal(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!form.clienteId || form.produtosSelecionados.length === 0 || !form.usuarioResponsavelId) return;
    setSaving(true);
    try {
      const produtosPayload = form.produtosSelecionados.map((produtoId) => ({
        produtoId,
        produtoModuloIds: form.modulosSelecionados[produtoId] ?? [],
      }));
      const payload: CreateHistoriaRequest = {
        clienteId: form.clienteId,
        usuarioResponsavelId: form.usuarioResponsavelId,
        status: form.status,
        tipo: form.tipo,
        dataInicio: form.dataInicio || null,
        dataFinalizacao: form.dataFinalizacao || null,
        observacoes: form.observacoes || null,
        produtos: produtosPayload,
      };
      let result: HistoriaDto;
      if (form.id) {
        const updatePayload: UpdateHistoriaRequest = { ...payload, id: form.id };
        result = await HistoriaService.update(form.id, updatePayload, empresaIdParam);
      } else {
        result = await HistoriaService.create(payload, empresaIdParam);
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
      const updated = await HistoriaService.adicionarMovimentacao(
        historia.id,
        { statusNovo: novoStatus },
        empresaIdParam
      );
      setHistorias((prev) => prev.map((h) => (h.id === updated.id ? updated : h)));
    } finally {
      setSaving(false);
    }
  };

  const handleDragStart = (historiaId: number) => (event: React.DragEvent<HTMLDivElement>) => {
    event.dataTransfer.setData("text/plain", historiaId.toString());
    event.dataTransfer.effectAllowed = "move";
  };

  const handleDragEnd = () => setDragOverStatus(null);

  const handleDragOverColumn = (status: HistoriaStatus) => (event: React.DragEvent<HTMLDivElement>) => {
    event.preventDefault();
    setDragOverStatus(status);
  };

  const handleDragLeaveColumn = () => setDragOverStatus(null);

  const handleDropOnColumn = (status: HistoriaStatus) => async (event: React.DragEvent<HTMLDivElement>) => {
    event.preventDefault();
    setDragOverStatus(null);
    const idValue = event.dataTransfer.getData("text/plain");
    const id = Number(idValue);
    if (!idValue || Number.isNaN(id)) return;
    const historia = historias.find((h) => h.id === id);
    if (historia && historia.status !== status) {
      await handleStatusChange(historia, status);
    }
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

  const produtosSelecionadosLista = useMemo(
    () => produtos.filter((p) => form.produtosSelecionados.includes(p.id)),
    [produtos, form.produtosSelecionados]
  );

  const modulosSelecionadosTotal = useMemo(() => {
    return produtosSelecionadosLista.reduce((total, produto) => {
      const selecionados = form.modulosSelecionados[produto.id] ?? [];
      return total + selecionados.length;
    }, 0);
  }, [produtosSelecionadosLista, form.modulosSelecionados]);

  const toggleProdutoSelecionado = (produtoId: number, selecionado: boolean) => {
    setForm((prev) => {
      const selecionados = new Set(prev.produtosSelecionados);
      if (selecionado) {
        selecionados.add(produtoId);
      } else {
        selecionados.delete(produtoId);
      }

      const { [produtoId]: _, ...restante } = prev.modulosSelecionados;

      return {
        ...prev,
        produtosSelecionados: Array.from(selecionados),
        modulosSelecionados: selecionado ? prev.modulosSelecionados : restante,
      };
    });
  };

  const limparSelecaoProdutos = () => {
    setForm((prev) => ({
      ...prev,
      produtosSelecionados: [],
      modulosSelecionados: {},
    }));
  };

  const renderCard = (historia: HistoriaDto) => {
    const ultimaMov: HistoriaMovimentacaoDto | undefined = historia.movimentacoes?.[0];
    const responsavel = historia.usuarioResponsavelNome || "Sem responsável";
    return (
      <div
        className="kanban-card"
        draggable
        onDragStart={handleDragStart(historia.id)}
        onDragEnd={handleDragEnd}
      >
        <div className="kanban-card-header">
          <div className="kanban-pill" style={{ backgroundColor: badgeTone[historia.status] }}>
            {statusLabels[historia.status]}
          </div>
        </div>
        <div className="kanban-title">{tipoLabels[historia.tipo]}</div>
        <div className="kanban-meta">
          <strong>Cliente:</strong> {historia.clienteNome}
        </div>
        <div className="kanban-meta kanban-products">
          <strong>Produtos:</strong>
          {historia.produtos.length === 0 ? (
            <span className="muted">Sem produtos vinculados</span>
          ) : (
            <ul className="kanban-product-list">
              {historia.produtos.map((produto) => (
                <li key={`${historia.id}-${produto.produtoId}`}>
                  <span>{produto.produtoNome}</span>
                  {produto.produtoModuloNomes.length > 0 && (
                    <span className="kanban-product-modules">{produto.produtoModuloNomes.join(", ")}</span>
                  )}
                </li>
              ))}
            </ul>
          )}
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
                <span className="kanban-column-title">
                  {statusIcons[col.status]} {statusLabels[col.status]}
                </span>
                <span className="kanban-count">{col.items.length}</span>
              </div>
              <div
                className={`kanban-column-body ${dragOverStatus === col.status ? "is-drag-over" : ""}`}
                onDragOver={handleDragOverColumn(col.status)}
                onDrop={handleDropOnColumn(col.status)}
                onDragLeave={handleDragLeaveColumn}
              >
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
                </div>
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
                <div className="form-row">
                  <label className="form-field form-field--full historia-field">
                    <div className="historia-field-head">
                      <div>
                        <div className="field-label-strong">Produtos (múltipla seleção)</div>
                        <p className="input-subtitle">Escolha um ou mais produtos para vincular a história</p>
                      </div>
                    </div>
                    <div className="product-search-bar">
                      <input
                        type="search"
                        className="search-input product-search"
                        placeholder="Buscar produto por nome"
                        value={produtoBusca}
                        onChange={(e) => setProdutoBusca(e.target.value)}
                      />
                    </div>
                    <div className="historia-panel product-selector-panel">
                      <div className="product-selector-layout">
                        <div className="product-selector-main">
                          <div className="product-list">
                            {produtosFiltrados.length === 0 ? (
                              <div className="product-empty">Nenhum produto encontrado para “{produtoBusca}”.</div>
                            ) : (
                              <div className="product-grid">
                                {produtosFiltrados.map((p) => {
                                  const selecionado = form.produtosSelecionados.includes(p.id);
                                  const modulosCount = p.modulos?.length ?? 0;
                                  const modulosSelecionadosProduto = form.modulosSelecionados[p.id] ?? [];
                                  const modulosSelecionadosSet = new Set(modulosSelecionadosProduto);
                                  return (
                                    <label key={p.id} className={`product-card ${selecionado ? "is-selected" : ""}`}>
                                      <input
                                        type="checkbox"
                                        checked={selecionado}
                                        onChange={(e) => toggleProdutoSelecionado(p.id, e.target.checked)}
                                      />
                                      <div className="product-check">
                                        <span className="product-check-icon">✓</span>
                                      </div>
                                      <div className="product-card-body">
                                        <div className="product-card-top">
                                          <div className="product-card-info">
                                            <span className="product-name">{p.nome}</span>
                                            {p.fornecedorNome && (
                                              <span className="product-supplier">Fornecedor: {p.fornecedorNome}</span>
                                            )}
                                          </div>
                                          <div className="product-card-meta">
                                            <span className="product-price">{currencyFormatter.format(p.valorRevenda)}</span>
                                            <span className="pill-badge">{selecionado ? "Selecionado" : "Disponível"}</span>
                                          </div>
                                        </div>
                                        <div className="product-meta">
                                          <span className="product-chip strong">
                                            {modulosCount} módulo{modulosCount === 1 ? "" : "s"}
                                          </span>
                                          <span className="product-chip">{modulosSelecionadosProduto.length} selecionado</span>
                                        </div>
                                        {selecionado && (
                                          <div className="product-modules-list">
                                            <div className="product-modules-header">
                                              <strong>Módulos</strong>
                                              <span>
                                                {modulosSelecionadosProduto.length}/{modulosCount} selecionados
                                              </span>
                                            </div>
                                            {modulosCount === 0 ? (
                                              <span className="product-module-empty">Nenhum módulo cadastrado.</span>
                                            ) : (
                                              <div className="product-module-grid">
                                                {(p.modulos ?? []).map((m) => (
                                                  <label key={`${p.id}-${m.id}`} className="module-option">
                                                    <input
                                                      type="checkbox"
                                                      checked={modulosSelecionadosSet.has(m.id)}
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
                                                    <div>
                                                      <span className="module-name">{m.nome}</span>
                                                      <span className="module-price">
                                                        {currencyFormatter.format(m.valorAdicional)}
                                                      </span>
                                                    </div>
                                                  </label>
                                                ))}
                                              </div>
                                            )}
                                          </div>
                                        )}
                                      </div>
                                    </label>
                                  );
                                })}
                              </div>
                            )}
                          </div>
                        </div>
                        <aside className="product-selector-summary">
                          <div className="summary-head">
                            <div>
                              <p className="summary-eyebrow">Resumo da seleção</p>
                              <span className="summary-total">
                                {produtosSelecionadosLista.length} produto
                                {produtosSelecionadosLista.length === 1 ? "" : "s"}
                              </span>
                            </div>
                            <span className="summary-pill">
                              {modulosSelecionadosTotal} módulo{modulosSelecionadosTotal === 1 ? "" : "s"}
                            </span>
                          </div>
                          {produtosSelecionadosLista.length === 0 ? (
                            <p className="summary-empty">Selecione um ou mais produtos para visualizar detalhes.</p>
                          ) : (
                            <ul className="summary-list">
                              {produtosSelecionadosLista.map((produto) => {
                                const modulosSelecionadosProduto = form.modulosSelecionados[produto.id] ?? [];
                                const modulosDoProduto = new Set(modulosSelecionadosProduto);
                                const moduloNomes =
                                  produto.modulos?.filter((m) => modulosDoProduto.has(m.id)).map((m) => m.nome) ?? [];
                                return (
                                  <li key={produto.id} className="summary-item">
                                    <div className="summary-item-head">
                                      <div>
                                        <strong>{produto.nome}</strong>
                                        <p className="summary-item-sub">
                                          {modulosSelecionadosProduto.length} módulo
                                          {modulosSelecionadosProduto.length === 1 ? "" : "s"} selecionado
                                          {modulosSelecionadosProduto.length === 1 ? "" : "s"}
                                        </p>
                                      </div>
                                      <button
                                        type="button"
                                        className="ghost small icon-only"
                                        onClick={() => toggleProdutoSelecionado(produto.id, false)}
                                        aria-label={`Remover ${produto.nome}`}
                                      >
                                        ×
                                      </button>
                                    </div>
                                    <div className="summary-modules">
                                      {moduloNomes.length === 0 ? (
                                        <span className="summary-chip muted">Nenhum módulo selecionado</span>
                                      ) : (
                                        moduloNomes.map((nome) => (
                                          <span key={nome} className="summary-chip">
                                            {nome}
                                          </span>
                                        ))
                                      )}
                                    </div>
                                    {produto.fornecedorNome && (
                                      <span className="summary-supplier">Fornecedor: {produto.fornecedorNome}</span>
                                    )}
                                  </li>
                                );
                              })}
                            </ul>
                          )}
                        </aside>
                      </div>
                    </div>
                  </label>
                </div>
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

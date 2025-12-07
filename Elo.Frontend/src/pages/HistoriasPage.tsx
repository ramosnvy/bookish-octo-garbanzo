import { useEffect, useMemo, useState } from "react";
import AppLayout from "../components/AppLayout";
import {
  ClienteDto,
  HistoriaDto,
  HistoriaMovimentacaoDto,
  HistoriaStatusConfigDto,
  HistoriaTipoConfigDto,
  UpdateHistoriaRequest,
  CreateHistoriaRequest,
  ProdutoDto,
} from "../models";
import { ClienteService } from "../services/ClienteService";
import { ProdutoService } from "../services/ProdutoService";
import { HistoriaService } from "../services/HistoriaService";
import { UserService } from "../services/UserService";
import { HistoriaStatusService } from "../services/HistoriaStatusService";
import { HistoriaTipoService } from "../services/HistoriaTipoService";
import { useAuth } from "../context/AuthContext";
import "../components.css";
import "../styles.css";

type FormState = {
  id?: number;
  clienteId: number;
  usuarioResponsavelId: number;
  statusId: number;
  tipoId: number;
  dataInicio?: string | null;
  dataFinalizacao?: string | null;
  observacoes?: string | null;
  produtosSelecionados: number[];
  modulosSelecionados: Record<number, number[]>;
};

type StatusDefinition = Pick<HistoriaStatusConfigDto, "id" | "nome" | "cor" | "ordem" | "fechaHistoria" | "ativo">;
type TipoDefinition = Pick<HistoriaTipoConfigDto, "id" | "nome" | "descricao" | "ordem" | "ativo">;
type KanbanColumn = {
  statusId: number;
  statusNome: string;
  statusCor?: string | null;
  items: HistoriaDto[];
};

const currencyFormatter = new Intl.NumberFormat("pt-BR", { style: "currency", currency: "BRL" });

const DEFAULT_STATUS_COLOR = "#475569";

const HistoriasPage = () => {
  const { selectedEmpresaId } = useAuth();
  const empresaIdParam = selectedEmpresaId ?? undefined;
  const [historias, setHistorias] = useState<HistoriaDto[]>([]);
  const [clientes, setClientes] = useState<ClienteDto[]>([]);
  const [produtos, setProdutos] = useState<ProdutoDto[]>([]);
  const [usuarios, setUsuarios] = useState<{ id: number; nome: string }[]>([]);
  const [statusOptions, setStatusOptions] = useState<HistoriaStatusConfigDto[]>([]);
  const [tipoOptions, setTipoOptions] = useState<HistoriaTipoConfigDto[]>([]);
  const [produtoBusca, setProdutoBusca] = useState("");
  const [loading, setLoading] = useState(false);
  const [erroCarregamento, setErroCarregamento] = useState<string | null>(null);
  const [saving, setSaving] = useState(false);
  const [showModal, setShowModal] = useState(false);
  const [editing, setEditing] = useState<HistoriaDto | null>(null);
  const [dragOverStatus, setDragOverStatus] = useState<number | null>(null);
  const [form, setForm] = useState<FormState>({
    clienteId: 0,
    usuarioResponsavelId: 0,
    statusId: 0,
    tipoId: 0,
    dataInicio: "",
    dataFinalizacao: "",
    observacoes: "",
    produtosSelecionados: [],
    modulosSelecionados: {},
  });
  const [filtroTipo, setFiltroTipo] = useState<number | "todos">("todos");

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
        const [hist, cli, prod, usr, statusList, tipoList] = await Promise.all([
          HistoriaService.getAll({ empresaId: selectedEmpresaId ?? undefined }),
          ClienteService.getAll(selectedEmpresaId ?? undefined),
          ProdutoService.getAll(selectedEmpresaId ?? undefined),
          UserService.getAll(selectedEmpresaId ?? undefined),
          HistoriaStatusService.getAll(selectedEmpresaId ?? undefined),
          HistoriaTipoService.getAll(selectedEmpresaId ?? undefined),
        ]);
        setHistorias(hist);
        setClientes(cli);
        setProdutos(prod);
        setUsuarios(usr.map((u) => ({ id: u.id, nome: u.nome })));
        setStatusOptions(statusList);
        setTipoOptions(tipoList);
      } catch (error) {
        console.error("Erro ao carregar dados de histórias", error);
        setErroCarregamento("Não foi possível carregar histórias, status ou cadastros relacionados. Verifique seu acesso/empresa.");
      } finally {
        setLoading(false);
      }
    };
    fetchData();
  }, [selectedEmpresaId]);

  const statusDefinitions = useMemo<StatusDefinition[]>(() => {
    const map = new Map<number, StatusDefinition>();
    statusOptions
      .slice()
      .sort((a, b) => a.ordem - b.ordem || a.nome.localeCompare(b.nome))
      .forEach(({ id, nome, cor, ordem, fechaHistoria, ativo }) => {
        map.set(id, { id, nome, cor, ordem, fechaHistoria, ativo });
      });
    historias.forEach((hist) => {
      if (!map.has(hist.statusId)) {
        map.set(hist.statusId, {
          id: hist.statusId,
          nome: hist.statusNome || "Sem status",
          cor: hist.statusCor ?? null,
          ordem: hist.statusId,
          fechaHistoria: hist.statusFechaHistoria ?? false,
          ativo: true,
        });
      }
    });
    return Array.from(map.values()).sort((a, b) => a.ordem - b.ordem || a.id - b.id);
  }, [statusOptions, historias]);

  const tipoDefinitions = useMemo<TipoDefinition[]>(() => {
    const map = new Map<number, TipoDefinition>();
    tipoOptions
      .slice()
      .sort((a, b) => a.ordem - b.ordem || a.nome.localeCompare(b.nome))
      .forEach(({ id, nome, descricao, ordem, ativo }) => {
        map.set(id, { id, nome, descricao, ordem, ativo });
      });
    historias.forEach((hist) => {
      if (!map.has(hist.tipoId)) {
        map.set(hist.tipoId, {
          id: hist.tipoId,
          nome: hist.tipoNome || "Sem tipo",
          descricao: hist.tipoDescricao ?? "",
          ordem: hist.tipoId,
          ativo: true,
        });
      }
    });
    return Array.from(map.values()).sort((a, b) => a.ordem - b.ordem || a.id - b.id);
  }, [tipoOptions, historias]);

  const selectableStatuses = useMemo(() => statusDefinitions.filter((status) => status.ativo), [statusDefinitions]);
  const selectableTipos = useMemo(() => tipoDefinitions.filter((tipo) => tipo.ativo), [tipoDefinitions]);
  const statusLookup = useMemo(() => new Map(statusDefinitions.map((status) => [status.id, status])), [statusDefinitions]);
  const tipoLookup = useMemo(() => new Map(tipoDefinitions.map((tipo) => [tipo.id, tipo])), [tipoDefinitions]);
  const defaultStatusId = selectableStatuses[0]?.id ?? statusDefinitions[0]?.id ?? 0;
  const defaultTipoId = selectableTipos[0]?.id ?? tipoDefinitions[0]?.id ?? 0;

  useEffect(() => {
    if (filtroTipo !== "todos" && !tipoDefinitions.some((tipo) => tipo.id === filtroTipo)) {
      setFiltroTipo("todos");
    }
  }, [filtroTipo, tipoDefinitions]);

  const resetForm = () => {
    setEditing(null);
    setForm({
      clienteId: 0,
      usuarioResponsavelId: 0,
      statusId: defaultStatusId,
      tipoId: defaultTipoId,
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
      usuarioResponsavelId: historia.usuarioResponsavelId ?? 0,
      statusId: historia.statusId,
      tipoId: historia.tipoId,
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
    if (
      !form.clienteId ||
      form.produtosSelecionados.length === 0 ||
      !form.usuarioResponsavelId ||
      !form.statusId ||
      !form.tipoId
    )
      return;
    setSaving(true);
    try {
      const produtosPayload = form.produtosSelecionados.map((produtoId) => ({
        produtoId,
        produtoModuloIds: form.modulosSelecionados[produtoId] ?? [],
      }));
      const payload: CreateHistoriaRequest = {
        clienteId: form.clienteId,
        usuarioResponsavelId: form.usuarioResponsavelId || null,
        statusId: form.statusId,
        tipoId: form.tipoId,
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

  const handleStatusChange = async (historia: HistoriaDto, novoStatusId: number) => {
    if (historia.statusId === novoStatusId) return;
    setSaving(true);
    try {
      const updated = await HistoriaService.adicionarMovimentacao(
        historia.id,
        { statusNovoId: novoStatusId },
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

  const handleDragOverColumn = (statusId: number) => (event: React.DragEvent<HTMLDivElement>) => {
    event.preventDefault();
    setDragOverStatus(statusId);
  };

  const handleDragLeaveColumn = () => setDragOverStatus(null);

  const handleDropOnColumn = (statusId: number) => async (event: React.DragEvent<HTMLDivElement>) => {
    event.preventDefault();
    setDragOverStatus(null);
    const idValue = event.dataTransfer.getData("text/plain");
    const id = Number(idValue);
    if (!idValue || Number.isNaN(id)) return;
    const historia = historias.find((h) => h.id === id);
    if (historia && historia.statusId !== statusId) {
      await handleStatusChange(historia, statusId);
    }
  };

  const historiasFiltradas = useMemo(() => {
    return historias.filter((h) => (filtroTipo === "todos" ? true : h.tipoId === filtroTipo));
  }, [historias, filtroTipo]);

  const columns = useMemo<KanbanColumn[]>(
    () =>
      statusDefinitions.map((status) => ({
        statusId: status.id,
        statusNome: status.nome,
        statusCor: status.cor,
        items: historiasFiltradas.filter((h) => h.statusId === status.id),
      })),
    [statusDefinitions, historiasFiltradas]
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
    const statusInfo = statusLookup.get(historia.statusId);
    const tipoInfo = tipoLookup.get(historia.tipoId);
    const badgeColor = statusInfo?.cor ?? historia.statusCor ?? DEFAULT_STATUS_COLOR;
    const statusName = statusInfo?.nome ?? historia.statusNome ?? "Sem status";
    const tipoName = tipoInfo?.nome ?? historia.tipoNome ?? "Sem tipo";
    return (
      <div
        className="kanban-card"
        draggable
        onDragStart={handleDragStart(historia.id)}
        onDragEnd={handleDragEnd}
      >
        <div className="kanban-card-header">
          <div className="kanban-pill" style={{ backgroundColor: badgeColor }}>
            {statusName}
          </div>
        </div>
        <div className="kanban-title">{tipoName}</div>
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
          <select value={filtroTipo} onChange={(e) => setFiltroTipo(e.target.value === "todos" ? "todos" : Number(e.target.value))}>
            <option value="todos">Todos os tipos</option>
            {tipoDefinitions.map((tipo) => (
              <option key={tipo.id} value={tipo.id}>
                {tipo.nome}
                {!tipo.ativo ? " (inativo)" : ""}
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
      ) : columns.length === 0 ? (
        <div className="card info">Configure ao menos um status de história para visualizar o kanban.</div>
      ) : (
        <div className="kanban-board">
          {columns.map((col) => (
            <div key={col.statusId} className="kanban-column">
              <div className="kanban-column-header">
                <span className="kanban-column-title">
                  <span className="color-dot" style={{ backgroundColor: col.statusCor ?? DEFAULT_STATUS_COLOR }} />
                  {col.statusNome}
                </span>
                <span className="kanban-count">{col.items.length}</span>
              </div>
              <div
                className={`kanban-column-body ${dragOverStatus === col.statusId ? "is-drag-over" : ""}`}
                onDragOver={handleDragOverColumn(col.statusId)}
                onDrop={handleDropOnColumn(col.statusId)}
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
                      value={form.tipoId}
                      onChange={(e) => setForm((f) => ({ ...f, tipoId: Number(e.target.value) }))}
                    >
                      {tipoDefinitions.length === 0 ? (
                        <option value={0}>Nenhum tipo disponível</option>
                      ) : (
                        tipoDefinitions.map((tipo) => (
                          <option key={tipo.id} value={tipo.id} disabled={!tipo.ativo}>
                            {tipo.nome}
                            {!tipo.ativo ? " (inativo)" : ""}
                          </option>
                        ))
                      )}
                    </select>
                  </label>
                  <label className="form-field">
                    Status
                    <select
                      value={form.statusId}
                      onChange={(e) => setForm((f) => ({ ...f, statusId: Number(e.target.value) }))}
                    >
                      {statusDefinitions.length === 0 ? (
                        <option value={0}>Nenhum status disponível</option>
                      ) : (
                        statusDefinitions.map((status) => (
                          <option key={status.id} value={status.id} disabled={!status.ativo}>
                            {status.nome}
                            {!status.ativo ? " (inativo)" : ""}
                          </option>
                        ))
                      )}
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

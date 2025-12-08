import { FormEvent, ReactNode, useEffect, useMemo, useState } from "react";
import AppLayout from "../components/AppLayout";
import {
  ClienteDto,
  TicketDto,
  TicketTipoConfigDto,
  TicketPrioridade,
  TicketStatus,
  CreateTicketRequest,
  UpdateTicketRequest,
  CreateRespostaTicketRequest,
} from "../models";
import { ClienteService } from "../services/ClienteService";
import { TicketService } from "../services/TicketService";
import { UserService } from "../services/UserService";
import { TicketTipoService } from "../services/TicketTipoService";
import { useAuth } from "../context/AuthContext";
import "../components.css";
import "../styles.css";

const ticketStatusOrder: TicketStatus[] = [
  TicketStatus.Aberto,
  TicketStatus.EmAndamento,
  TicketStatus.PendenteCliente,
  TicketStatus.Resolvido,
  TicketStatus.Fechado,
  TicketStatus.Cancelado,
];

const ticketStatusLabels: Record<TicketStatus, string> = {
  [TicketStatus.Aberto]: "Aberto",
  [TicketStatus.EmAndamento]: "Em andamento",
  [TicketStatus.PendenteCliente]: "Pendente cliente",
  [TicketStatus.Resolvido]: "Resolvido",
  [TicketStatus.Fechado]: "Fechado",
  [TicketStatus.Cancelado]: "Cancelado",
};

const ticketPriorityLabels: Record<TicketPrioridade, string> = {
  [TicketPrioridade.Baixa]: "Baixa",
  [TicketPrioridade.Media]: "Média",
  [TicketPrioridade.Alta]: "Alta",
  [TicketPrioridade.Critica]: "Crítica",
};

const IconDiscussion = () => (
  <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" strokeWidth="1.6">
    <path d="M4 4h16v10H6l-3 3V4z" strokeLinecap="round" strokeLinejoin="round" />
  </svg>
);

const IconEdit = () => (
  <svg viewBox="0 0 24 24" width="14" height="14" fill="none" stroke="currentColor" strokeWidth="1.6">
    <path d="M5 17.5V19h1.5l7.2-7.2-1.5-1.5L5 17.5z" strokeLinecap="round" strokeLinejoin="round" />
    <path d="M14.4 6.1l3.5 3.5L7.9 19H4.5v-3.4l10-9.5z" strokeLinecap="round" strokeLinejoin="round" />
    <path d="M16.9 3.6l3.5 3.5" strokeLinecap="round" />
  </svg>
);

const statusTone: Record<TicketStatus, string> = {
  [TicketStatus.Aberto]: "#eab308",
  [TicketStatus.EmAndamento]: "#0ea5e9",
  [TicketStatus.PendenteCliente]: "#f97316",
  [TicketStatus.Resolvido]: "#22c55e",
  [TicketStatus.Fechado]: "#14b8a6",
  [TicketStatus.Cancelado]: "#ef4444",
};

type TicketForm = {
  id?: number;
  clienteId: number;
  titulo: string;
  descricao: string;
  ticketTipoId: number;
  prioridade: TicketPrioridade;
  status: TicketStatus;
  usuarioAtribuidoId?: number | null;
  numeroExterno: string;
  dataFechamento?: string;
};

const initialForm: TicketForm = {
  clienteId: 0,
  titulo: "",
  descricao: "",
  ticketTipoId: 0,
  prioridade: TicketPrioridade.Media,
  status: TicketStatus.Aberto,
  usuarioAtribuidoId: null,
  numeroExterno: "",
};

type DurationTone = "short" | "medium" | "long";

type DurationInfo = {
  text: string;
  tone: DurationTone;
  diff: number;
};

const formatDateTime = (value: string) =>
  new Date(value).toLocaleString("pt-BR", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  });

const getDurationInfo = (start: string, now = Date.now()): DurationInfo => {
  const startTime = new Date(start).getTime();
  const diffMs = Math.max(now - startTime, 0);
  const days = Math.floor(diffMs / 86_400_000);
  const hours = Math.floor((diffMs % 86_400_000) / 3_600_000);
  const minutes = Math.floor((diffMs % 3_600_000) / 60_000);
  const parts: string[] = [];
  if (days) parts.push(`${days}d`);
  if (hours) parts.push(`${hours}h`);
  if (!parts.length) {
    if (minutes) parts.push(`${minutes}m`);
    else parts.push("<1m");
  }
  const totalHours = diffMs / 3_600_000;
  let tone: DurationTone = "short";
  if (totalHours >= 72) tone = "long";
  else if (totalHours >= 24) tone = "medium";
  return {
    text: parts.join(" "),
    tone,
    diff: diffMs,
  };
};

const TicketsPage = () => {
  const { selectedEmpresaId } = useAuth();
  const empresaIdParam = selectedEmpresaId ?? undefined;
  const [tickets, setTickets] = useState<TicketDto[]>([]);
  const [clientes, setClientes] = useState<ClienteDto[]>([]);
  const [usuarios, setUsuarios] = useState<{ id: number; nome: string }[]>([]);
  const [ticketTipos, setTicketTipos] = useState<TicketTipoConfigDto[]>([]);
  const [loading, setLoading] = useState(false);
  const [form, setForm] = useState<TicketForm>(initialForm);
  const [selectedTicket, setSelectedTicket] = useState<TicketDto | null>(null);
  const [discussionTicket, setDiscussionTicket] = useState<TicketDto | null>(null);
  const [responseMessage, setResponseMessage] = useState("");
  const [responseInternal, setResponseInternal] = useState(true);
  const [responseSubmitting, setResponseSubmitting] = useState(false);
  const [filterStatus, setFilterStatus] = useState<TicketStatus | "todos">("todos");
  const [filterResponsavel, setFilterResponsavel] = useState<number | "">("");
  const [searchTicketNumber, setSearchTicketNumber] = useState("");
  const [saving, setSaving] = useState(false);
  const [showModal, setShowModal] = useState(false);

  const activeTicketTipos = useMemo(
    () =>
      ticketTipos
        .filter((tipo) => tipo.ativo)
        .sort((a, b) => a.ordem - b.ordem || a.id - b.id),
    [ticketTipos]
  );

  const loadLookupData = async (force = false) => {
    const shouldLoadClientes = force || clientes.length === 0;
    const shouldLoadUsuarios = force || usuarios.length === 0;
    const shouldLoadTicketTipos = force || ticketTipos.length === 0;

    if (!shouldLoadClientes && !shouldLoadUsuarios && !shouldLoadTicketTipos) {
      return;
    }

    try {
      const [cli, usr, tipos] = await Promise.all([
        shouldLoadClientes ? ClienteService.getAll(empresaIdParam) : Promise.resolve<ClienteDto[] | null>(null),
        shouldLoadUsuarios ? UserService.getAll(empresaIdParam) : Promise.resolve(null),
        shouldLoadTicketTipos ? TicketTipoService.getAll(empresaIdParam) : Promise.resolve<TicketTipoConfigDto[] | null>(null),
      ]);

      if (cli) {
        setClientes(cli);
      }
      if (usr) {
        setUsuarios(usr.map((u) => ({ id: u.id, nome: u.nome })));
      }
      if (tipos) {
        setTicketTipos(tipos);
      }
    } catch (error) {
      console.error("Erro ao carregar clientes, funcionários ou tipos de ticket", error);
    }
  };

  const fetchTickets = async () => {
    setLoading(true);
    try {
      const tick = await TicketService.getAll({ empresaId: selectedEmpresaId ?? undefined });
      setTickets(tick);
    } catch (error) {
      console.error("Erro ao carregar tickets", error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadLookupData(true);
    fetchTickets();
  }, [selectedEmpresaId]);

  useEffect(() => {
    if (activeTicketTipos.length > 0 && !form.ticketTipoId) {
      setForm((prev) => ({
        ...prev,
        ticketTipoId: prev.ticketTipoId || activeTicketTipos[0].id,
      }));
    }
  }, [activeTicketTipos, form.ticketTipoId]);

  const setFormFromTicket = (ticket: TicketDto) => {
    setForm({
      id: ticket.id,
      clienteId: ticket.clienteId,
      titulo: ticket.titulo,
      descricao: ticket.descricao,
      ticketTipoId: ticket.ticketTipoId,
      prioridade: ticket.prioridade,
      status: ticket.status,
      usuarioAtribuidoId: ticket.usuarioAtribuidoId ?? null,
      numeroExterno: ticket.numeroExterno,
      dataFechamento: ticket.dataFechamento ?? undefined,
    });
  };

  const resetForm = () => {
    setForm({
      ...initialForm,
      ticketTipoId: activeTicketTipos[0]?.id ?? 0,
    });
  };

  const clearSelection = () => {
    setSelectedTicket(null);
    setResponseMessage("");
    setResponseInternal(true);
  };

  const handleNewTicket = async () => {
    resetForm();
    clearSelection();
    await loadLookupData();
    setShowModal(true);
  };

  const openEditModal = async (ticket: TicketDto) => {
    await loadLookupData();
    setFormFromTicket(ticket);
    setSelectedTicket(ticket);
    setResponseMessage("");
    setResponseInternal(true);
    setShowModal(true);
  };

  const openDiscussionModal = (ticket: TicketDto) => {
    setDiscussionTicket(ticket);
    setResponseMessage("");
    setResponseInternal(true);
  };

  const closeModal = () => setShowModal(false);
  const closeDiscussionModal = () => {
    setDiscussionTicket(null);
    setResponseMessage("");
    setResponseInternal(true);
  };

  const handleSubmit = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!form.clienteId || !form.titulo || !form.ticketTipoId) return;
    setSaving(true);
    const payload: CreateTicketRequest = {
      clienteId: form.clienteId,
      ticketTipoId: form.ticketTipoId,
      titulo: form.titulo,
      descricao: form.descricao,
      prioridade: form.prioridade,
      status: form.status,
      usuarioAtribuidoId: form.usuarioAtribuidoId,
      numeroExterno: form.numeroExterno,
    };
    try {
      let result: TicketDto;
      if (form.id) {
        const updatePayload: UpdateTicketRequest = {
          ...payload,
          id: form.id,
          dataFechamento: form.dataFechamento,
        };
        result = await TicketService.update(form.id, updatePayload, empresaIdParam);
        setTickets((prev) => prev.map((t) => (t.id === result.id ? result : t)));
      } else {
        result = await TicketService.create(payload, empresaIdParam);
        setTickets((prev) => [result, ...prev]);
      }
      setSelectedTicket(result);
      setFormFromTicket(result);
      setResponseMessage("");
      setResponseInternal(true);
      setShowModal(false);
    } finally {
      setSaving(false);
    }
  };

  const handleResponseSubmit = async () => {
    if (!discussionTicket || !responseMessage.trim()) return;
    setResponseSubmitting(true);
    try {
      const result = await TicketService.respond(
        discussionTicket.id,
        { mensagem: responseMessage.trim(), isInterna: responseInternal },
        empresaIdParam
      );
      setTickets((prev) => prev.map((t) => (t.id === result.id ? result : t)));
      setDiscussionTicket(result);
      if (selectedTicket && selectedTicket.id === result.id) {
        setSelectedTicket(result);
      }
      setFormFromTicket(result);
      setResponseMessage("");
    } finally {
      setResponseSubmitting(false);
    }
  };

  const filteredTickets = useMemo(() => {
    return tickets.filter((ticket) => {
      const statusMatch = filterStatus === "todos" || ticket.status === filterStatus;
      const responsavelMatch =
        !filterResponsavel || ticket.usuarioAtribuidoId === Number(filterResponsavel);
      const numberMatch = !searchTicketNumber
        || ticket.numeroExterno?.toLowerCase().includes(searchTicketNumber.toLowerCase())
        || ticket.id.toString() === searchTicketNumber.replace(/\D/g, "");
      return statusMatch && responsavelMatch && numberMatch;
    });
  }, [filterStatus, filterResponsavel, searchTicketNumber, tickets]);

  const buildMetaLabel = (title: string, value: ReactNode, key?: string) => (
    <span key={key}>
      <strong>{title}: </strong>
      {value}
    </span>
  );

  const discussionDurationInfo = discussionTicket
    ? getDurationInfo(discussionTicket.dataAbertura)
    : null;

  const buildUpdatePayload = (ticket: TicketDto, status: TicketStatus): UpdateTicketRequest => ({
    id: ticket.id,
    clienteId: ticket.clienteId,
    titulo: ticket.titulo,
    descricao: ticket.descricao,
    ticketTipoId: ticket.ticketTipoId,
    prioridade: ticket.prioridade,
    status,
    usuarioAtribuidoId: ticket.usuarioAtribuidoId,
    numeroExterno: ticket.numeroExterno,
    dataFechamento: ticket.dataFechamento ?? undefined,
  });

  const handleStatusChange = async (ticket: TicketDto, status: TicketStatus) => {
    if (ticket.status === status) return;
    const payload = buildUpdatePayload(ticket, status);
    const updated = await TicketService.update(ticket.id, payload, empresaIdParam);
    setTickets((prev) => prev.map((t) => (t.id === updated.id ? updated : t)));
    if (discussionTicket?.id === updated.id) {
      setDiscussionTicket(updated);
    }
    if (selectedTicket?.id === updated.id) {
      setSelectedTicket(updated);
    }
  };

  const pageActions = (
    <button type="button" className="primary" onClick={handleNewTicket}>
      Novo ticket
    </button>
  );

  return (
    <AppLayout
      title="Tickets"
      subtitle="Painel tipo fórum para criar, atualizar e discutir chamados."
      actions={pageActions}
    >
      <div className="tickets-page">
        <section className="card ticket-card">
          <div className="ticket-card-header">
            <div>
              <p className="modal-eyebrow">Fila</p>
              <h3>Tickets</h3>
            </div>
            <div className="ticket-card-actions">
              <label className="ticket-filter-label">
                Status
                <select
                  value={filterStatus}
                  onChange={(e) =>
                    setFilterStatus(e.target.value === "todos" ? "todos" : (Number(e.target.value) as TicketStatus))
                  }
                >
                  <option value="todos">Todos os status</option>
                  {ticketStatusOrder.map((status) => (
                    <option key={status} value={status}>
                      {ticketStatusLabels[status]}
                    </option>
                  ))}
                </select>
              </label>
              <label className="ticket-filter-label">
                Responsável
                <select
                  value={filterResponsavel}
                  onChange={(e) => setFilterResponsavel(e.target.value ? Number(e.target.value) : "")}
                >
                  <option value="">Todos</option>
                  {usuarios.map((u) => (
                    <option key={u.id} value={u.id}>
                      {u.nome}
                    </option>
                  ))}
                </select>
              </label>
              <label className="ticket-filter-label ticket-search-field">
                Ticket
                <input
                  className="ticket-search-input"
                  value={searchTicketNumber}
                  onChange={(e) => setSearchTicketNumber(e.target.value)}
                  placeholder="Número ou #"
                />
              </label>
            </div>
          </div>

          <div className="ticket-card-body">
            {loading ? (
              <div className="card">Carregando tickets...</div>
            ) : (
              <div className="ticket-table-wrapper">
                <table className="ticket-table">
                  <thead>
                    <tr>
                      <th>Número</th>
                      <th>Título</th>
                      <th>Cliente</th>
                      <th>Tipo</th>
                      <th>Status</th>
                      <th>Prioridade</th>
                      <th>Responsável</th>
                      <th>Criado</th>
                      <th>Tempo aberto</th>
                      <th>Atualizado</th>
                      <th />
                    </tr>
                  </thead>
                  <tbody>
                    {filteredTickets.length === 0 ? (
                      <tr>
                        <td colSpan={11} className="ticket-table-empty">
                          Nenhum ticket encontrado para o filtro selecionado.
                        </td>
                      </tr>
                    ) : (
                      filteredTickets.map((ticket) => (
                        <tr
                          key={ticket.id}
                          className={`ticket-table-row ${selectedTicket?.id === ticket.id ? "is-active" : ""}`}
                        >
                          <td>#{ticket.numeroExterno || ticket.id}</td>
                          <td>{ticket.titulo}</td>
                          <td>{ticket.clienteNome}</td>
                          <td>{ticket.ticketTipoNome || "—"}</td>
                          <td>
                            <div className="ticket-status-cell">
                              <select
                                className="status-select dropdown-button"
                                style={{
                                  backgroundColor: statusTone[ticket.status],
                                  color: "#fff",
                                }}
                                value={ticket.status}
                                onChange={(e) =>
                                  handleStatusChange(ticket, Number(e.target.value) as TicketStatus)
                                }
                              >
                                {ticketStatusOrder.map((status) => (
                                  <option key={status} value={status}>
                                    {ticketStatusLabels[status]}
                                  </option>
                                ))}
                              </select>
                            </div>
                          </td>
                          <td>{ticketPriorityLabels[ticket.prioridade]}</td>
                          <td>{ticket.usuarioAtribuidoNome ?? "—"}</td>
                          <td>{formatDateTime(ticket.dataAbertura)}</td>
                          <td>
                            {(() => {
                              const durationInfo = getDurationInfo(ticket.dataAbertura);
                              return (
                                <span className={`ticket-duration ticket-duration--${durationInfo.tone}`}>
                                  {durationInfo.text}
                                </span>
                              );
                            })()}
                          </td>
                          <td>{formatDateTime(ticket.updatedAt ?? ticket.createdAt)}</td>
                          <td>
                            <div className="ticket-row-actions">
                              <button
                                type="button"
                                className="ghost icon-only secondary ticket-row-action-button"
                                onClick={() => openDiscussionModal(ticket)}
                                aria-label="Abrir discussão"
                                title="Abrir discussão"
                              >
                                <IconDiscussion />
                              </button>
                              <button
                                type="button"
                                className="ghost icon-only primary ticket-row-action-button"
                                onClick={() => openEditModal(ticket)}
                                aria-label="Editar ticket"
                                title="Editar ticket"
                              >
                                <IconEdit />
                              </button>
                            </div>
                          </td>
                        </tr>
                      ))
                    )}
                  </tbody>
                </table>
              </div>
            )}
          </div>
        </section>
        {showModal && (
          <div className="modal-overlay">
            <div className="modal historia-modal">
              <div className="modal-header">
                <div>
                  <p className="modal-eyebrow">{form.id ? "Editar ticket" : "Novo ticket"}</p>
                  <h2>{form.id ? "Atualizar" : "Cadastrar"} ticket</h2>
                </div>
                <button className="ghost icon-only" type="button" onClick={closeModal}>
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
                        onChange={(e) => setForm((prev) => ({ ...prev, clienteId: Number(e.target.value) }))}
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
                    <label className="form-field">
                      Responsável
                      <select
                        value={form.usuarioAtribuidoId ?? ""}
                        onChange={(e) =>
                          setForm((prev) => ({
                            ...prev,
                            usuarioAtribuidoId: e.target.value ? Number(e.target.value) : null,
                          }))
                        }
                      >
                        <option value="">Sem responsável</option>
                        {usuarios.map((u) => (
                          <option key={u.id} value={u.id}>
                            {u.nome}
                          </option>
                        ))}
                      </select>
                    </label>
                  </div>
                  <label className="form-field">
                    Título
                    <input
                      value={form.titulo}
                      onChange={(e) => setForm((prev) => ({ ...prev, titulo: e.target.value }))}
                      required
                    />
                  </label>
                  <label className="form-field">
                    Descrição
                    <textarea
                      value={form.descricao}
                      onChange={(e) => setForm((prev) => ({ ...prev, descricao: e.target.value }))}
                      rows={4}
                    />
                  </label>
                  <div className="form-row">
                    <label className="form-field">
                      Tipo
                      <select
                        value={form.ticketTipoId || ""}
                        onChange={(e) =>
                          setForm((prev) => ({
                            ...prev,
                            ticketTipoId: e.target.value ? Number(e.target.value) : 0,
                          }))
                        }
                        required
                        disabled={!activeTicketTipos.length}
                      >
                        {activeTicketTipos.length === 0 ? (
                          <option value="">Cadastre tipos de ticket nas configurações</option>
                        ) : (
                          activeTicketTipos.map((tipo) => (
                            <option key={tipo.id} value={tipo.id}>
                              {tipo.nome}
                            </option>
                          ))
                        )}
                      </select>
                    </label>
                    <label className="form-field">
                      Prioridade
                      <select
                        value={form.prioridade}
                        onChange={(e) =>
                          setForm((prev) => ({ ...prev, prioridade: Number(e.target.value) as TicketPrioridade }))
                        }
                      >
                        {Object.values(TicketPrioridade)
                          .filter((value) => typeof value === "number")
                          .map((value) => (
                            <option key={value} value={value as number}>
                              {ticketPriorityLabels[value as TicketPrioridade]}
                            </option>
                          ))}
                      </select>
                    </label>
                  </div>
                  <div className="form-row">
                    <label className="form-field">
                      Status
                      <select
                        value={form.status}
                        onChange={(e) => setForm((prev) => ({ ...prev, status: Number(e.target.value) as TicketStatus }))}
                      >
                        {ticketStatusOrder.map((status) => (
                          <option key={status} value={status}>
                            {ticketStatusLabels[status]}
                          </option>
                        ))}
                      </select>
                    </label>
                    <label className="form-field">
                      Número externo
                      <input
                        value={form.numeroExterno}
                        onChange={(e) => setForm((prev) => ({ ...prev, numeroExterno: e.target.value }))}
                      />
                    </label>
                  </div>
                  <div className="ticket-modal-actions">
                    <button type="button" className="ghost" onClick={closeModal}>
                      Cancelar
                    </button>
                    <button type="submit" className="primary" disabled={saving}>
                      {saving ? "Salvando..." : form.id ? "Atualizar ticket" : "Criar ticket"}
                    </button>
                  </div>
                </form>
              </div>
            </div>
          </div>
        )}

        {discussionTicket && (
          <div className="modal-overlay">
            <div className="modal historia-modal">
              <div className="modal-header">
                <div>
                  <p className="modal-eyebrow">Discussão</p>
                  <h2>{discussionTicket.titulo}</h2>
                </div>
                <button className="ghost icon-only" type="button" onClick={closeDiscussionModal}>
                  ×
                </button>
              </div>
              <div className="modal-content">
                <section className="ticket-forum-card">
                  <div className="ticket-forum-header">
                    <div>
                      <p className="modal-eyebrow">Detalhes</p>
                      <h3>{discussionTicket.titulo}</h3>
                    </div>
                    <span
                      className="ticket-status-pill"
                      style={{ backgroundColor: statusTone[discussionTicket.status] }}
                    >
                      {ticketStatusLabels[discussionTicket.status]}
                    </span>
                  </div>
                  <div className="ticket-forum-meta">
                    {(() => {
                      const meta: ReactNode[] = [];
                      meta.push(buildMetaLabel("Cliente", discussionTicket.clienteNome, "cliente"));
                      meta.push(
                        buildMetaLabel(
                          "Tipo",
                          discussionTicket.ticketTipoNome || `Tipo #${discussionTicket.ticketTipoId}`,
                          "tipo"
                        )
                      );
                      meta.push(
                        buildMetaLabel(
                          "Prioridade",
                          ticketPriorityLabels[discussionTicket.prioridade],
                          "prioridade"
                        )
                      );
                      meta.push(
                        buildMetaLabel(
                          "Responsável",
                          discussionTicket.usuarioAtribuidoNome ?? "Sem responsável",
                          "responsavel"
                        )
                      );
                      meta.push(
                        buildMetaLabel(
                          "Criado em",
                          formatDateTime(discussionTicket.dataAbertura),
                          "criado"
                        )
                      );
                      if (discussionDurationInfo) {
                        meta.push(
                          buildMetaLabel(
                            "Tempo aberto",
                            <span
                              className={`ticket-duration ticket-duration--${discussionDurationInfo.tone}`}
                            >
                              {discussionDurationInfo.text}
                            </span>,
                            "tempo"
                          )
                        );
                      }
                      return meta;
                    })()}
                    <button
                      type="button"
                      className="ghost small"
                      onClick={() => openEditModal(discussionTicket)}
                    >
                      Editar
                    </button>
                  </div>

                  <div className="ticket-posts">
                    <article className="ticket-forum-post">
                      <div className="ticket-forum-post-header">
                        <strong>{discussionTicket.clienteNome}</strong>
                        <span className="muted">{formatDateTime(discussionTicket.dataAbertura)}</span>
                      </div>
                      <p>{discussionTicket.descricao || "Sem descrição disponível."}</p>
                    </article>
                    {discussionTicket.respostas.length === 0 ? (
                      <p className="ticket-forum-empty">Nenhuma atualização registrada.</p>
                    ) : (
                      discussionTicket.respostas.map((resp) => (
                        <article key={resp.id} className="ticket-forum-post response">
                          <div className="ticket-forum-post-header">
                            <strong>{resp.usuarioNome}</strong>
                            <span className="muted">{formatDateTime(resp.dataResposta)}</span>
                          </div>
                          <div className="ticket-forum-response-flag">
                            <span className="ticket-status-pill ticket-status-pill--soft">
                              {resp.isInterna ? "Interna" : "Externa"}
                            </span>
                          </div>
                          <p>{resp.mensagem}</p>
                        </article>
                      ))
                    )}
                  </div>

                  <div className="ticket-response-form">
                    <label className="form-field form-field--full">
                      Adicionar atualização
                      <textarea
                        value={responseMessage}
                        onChange={(e) => setResponseMessage(e.target.value)}
                        rows={4}
                        placeholder="Descreva o que foi feito, pendências ou fechamentos..."
                      />
                    </label>
                    <div className="ticket-response-actions">
                      <label className="form-field">
                        Visibilidade
                        <select
                          value={responseInternal ? "true" : "false"}
                          onChange={(e) => setResponseInternal(e.target.value === "true")}
                        >
                          <option value="true">Interna</option>
                          <option value="false">Externa</option>
                        </select>
                      </label>
                      <button
                        type="button"
                        className="primary"
                        onClick={handleResponseSubmit}
                        disabled={responseSubmitting}
                      >
                        {responseSubmitting ? "Registrando..." : "Registrar atualização"}
                      </button>
                    </div>
                  </div>
                </section>
              </div>
            </div>
          </div>
        )}

      </div>
    </AppLayout>
  );
};

export default TicketsPage;

import { FormEvent, useCallback, useEffect, useMemo, useState } from "react";
import AppLayout from "../components/AppLayout";
import { useAuth } from "../context/AuthContext";
import {
  CreateHistoriaStatusConfigRequest,
  CreateHistoriaTipoConfigRequest,
  CreateTicketTipoConfigRequest,
  HistoriaStatusConfigDto,
  HistoriaTipoConfigDto,
  TicketTipoConfigDto,
} from "../models";
import { HistoriaStatusService } from "../services/HistoriaStatusService";
import { HistoriaTipoService } from "../services/HistoriaTipoService";
import { TicketTipoService } from "../services/TicketTipoService";

type TabKey = "historia-status" | "historia-tipos" | "ticket-tipos";

const tabOptions: Record<TabKey, string> = {
  "historia-status": "Status de histórias",
  "historia-tipos": "Tipos de histórias",
  "ticket-tipos": "Tipos de tickets",
};

interface ConfigTabProps {
  empresaId?: number | null;
}

type BannerMessage = { type: "info" | "error"; text: string } | null;

const useBodyScrollLock = (open: boolean) => {
  useEffect(() => {
    if (open) {
      document.body.classList.add("modal-open");
    } else {
      document.body.classList.remove("modal-open");
    }
    return () => document.body.classList.remove("modal-open");
  }, [open]);
};

const useAsyncList = <T,>(loader: () => Promise<T[]>) => {
  const [items, setItems] = useState<T[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await loader();
      setItems(data);
    } catch (err) {
      console.error(err);
      setError("Não foi possível carregar os dados.");
    } finally {
      setLoading(false);
    }
  }, [loader]);

  return { items, setItems, loading, error, load };
};

type HistoriaStatusForm = Omit<CreateHistoriaStatusConfigRequest, "ordem"> & {
  ordem: string;
  id?: number | null;
};

const HistoriaStatusTab = ({ empresaId }: ConfigTabProps) => {
  const initialForm: HistoriaStatusForm = {
    id: null,
    nome: "",
    descricao: "",
    cor: "",
    fechaHistoria: false,
    ordem: "1",
    ativo: true,
  };
  const [form, setForm] = useState(initialForm);
  const [saving, setSaving] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);
  const [modalError, setModalError] = useState<string | null>(null);
  const [bannerMessage, setBannerMessage] = useState<BannerMessage>(null);

  const { items, setItems, loading, error, load } = useAsyncList<HistoriaStatusConfigDto>(
    useCallback(() => HistoriaStatusService.getAll(empresaId), [empresaId])
  );

  useEffect(() => {
    load();
  }, [load]);

  useBodyScrollLock(modalOpen);

  const resetForm = () => setForm(initialForm);

  const closeModal = () => {
    setModalOpen(false);
    setModalError(null);
    resetForm();
  };

  const openCreate = () => {
    resetForm();
    setModalOpen(true);
  };

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setSaving(true);
    setModalError(null);
    try {
      const payload: CreateHistoriaStatusConfigRequest = {
        nome: form.nome,
        descricao: form.descricao,
        cor: form.cor,
        fechaHistoria: form.fechaHistoria,
        ordem: form.ordem.trim() === "" ? 0 : Number(form.ordem),
        ativo: form.ativo,
      };
      if (form.id) {
        const updated = await HistoriaStatusService.update(form.id, payload, empresaId);
        setItems((prev) => prev.map((item) => (item.id === updated.id ? updated : item)));
        setBannerMessage({ type: "info", text: "Status atualizado com sucesso." });
      } else {
        const created = await HistoriaStatusService.create(payload, empresaId);
        setItems((prev) => [...prev, created]);
        setBannerMessage({ type: "info", text: "Status criado com sucesso." });
      }
      closeModal();
    } catch (err) {
      console.error(err);
      setModalError("Não foi possível salvar o status. Tente novamente.");
    } finally {
      setSaving(false);
    }
  };

  const handleEdit = (status: HistoriaStatusConfigDto) => {
    setForm({
      id: status.id,
      nome: status.nome,
      descricao: status.descricao ?? "",
      cor: status.cor ?? "",
      fechaHistoria: status.fechaHistoria,
      ordem: String(status.ordem),
      ativo: status.ativo,
    });
    setModalOpen(true);
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm("Deseja realmente excluir este status?")) {
      return;
    }
    try {
      await HistoriaStatusService.remove(id, empresaId);
      setItems((prev) => prev.filter((item) => item.id !== id));
      setBannerMessage({ type: "info", text: "Status removido com sucesso." });
    } catch (err) {
      console.error(err);
      setBannerMessage({ type: "error", text: "Não foi possível excluir o status." });
    }
  };

  return (
    <div className="card stack">
      <div className="filters-bar">
        <div className="filters-actions">
          <button type="button" className="primary" onClick={openCreate}>
            + Novo status
          </button>
        </div>
        {bannerMessage && (
          <p className={bannerMessage.type === "error" ? "danger-text" : "info-text"}>{bannerMessage.text}</p>
        )}
      </div>

      <div className="table-wrapper">
        {loading && <p>Carregando status...</p>}
        {error && <p className="danger-text">{error}</p>}
        {!loading && !error && (
          <table>
            <thead>
              <tr>
                <th>Nome</th>
                <th>Ordem</th>
                <th>Fecha história?</th>
                <th>Ativo</th>
                <th>Ações</th>
              </tr>
            </thead>
            <tbody>
              {items.length === 0 && (
                <tr>
                  <td colSpan={5}>Nenhum status cadastrado.</td>
                </tr>
              )}
              {items
                .slice()
                .sort((a, b) => a.ordem - b.ordem || a.id - b.id)
                .map((status) => (
                  <tr key={status.id}>
                    <td>
                      <div className="badge-row">
                        {status.cor && <span className="color-dot" style={{ backgroundColor: status.cor }} />}
                        <span>{status.nome}</span>
                      </div>
                    </td>
                    <td>{status.ordem}</td>
                    <td>{status.fechaHistoria ? "Sim" : "Não"}</td>
                    <td>{status.ativo ? "Sim" : "Não"}</td>
                    <td className="table-actions">
                      <button type="button" className="ghost" onClick={() => handleEdit(status)}>
                        Editar
                      </button>
                      <button type="button" className="danger ghost" onClick={() => handleDelete(status.id)}>
                        Excluir
                      </button>
                    </td>
                  </tr>
                ))}
            </tbody>
          </table>
        )}
      </div>

      {modalOpen && (
        <div className="modal-overlay" onClick={closeModal}>
          <div className="modal small" onClick={(event) => event.stopPropagation()}>
            <div className="modal-header">
              <div>
                <p className="modal-eyebrow">Status de história</p>
                <h3>{form.id ? "Editar status" : "Novo status"}</h3>
              </div>
              <button type="button" className="ghost" onClick={closeModal}>
                Fechar
              </button>
            </div>
            <div className="modal-content">
              <form className="modal-form" onSubmit={handleSubmit}>
                <div className="modal-inline-fields">
                  <div className="form-field">
                    <label htmlFor="hist-status-nome">Nome</label>
                    <input
                      id="hist-status-nome"
                      type="text"
                      value={form.nome}
                      onChange={(event) => setForm((prev) => ({ ...prev, nome: event.target.value }))}
                      required
                    />
                  </div>
                  <div className="form-field">
                    <label htmlFor="hist-status-descricao">Descrição</label>
                    <input
                      id="hist-status-descricao"
                      type="text"
                      value={form.descricao ?? ""}
                      onChange={(event) => setForm((prev) => ({ ...prev, descricao: event.target.value }))}
                    />
                  </div>
                </div>
                <div className="modal-inline-fields">
                  <div className="form-field">
                    <label htmlFor="hist-status-cor">Cor (hex)</label>
                    <input
                      id="hist-status-cor"
                      type="text"
                      value={form.cor ?? ""}
                      onChange={(event) => setForm((prev) => ({ ...prev, cor: event.target.value }))}
                      placeholder="#1d4ed8"
                    />
                  </div>
                  <div className="form-field">
                    <label htmlFor="hist-status-ordem">Ordem</label>
                    <input
                      id="hist-status-ordem"
                      type="number"
                      min={0}
                      value={form.ordem}
                      onChange={(event) => setForm((prev) => ({ ...prev, ordem: event.target.value }))}
                    />
                  </div>
                </div>
                <div className="modal-checkbox-row">
                  <label className="checkbox">
                    <input
                      type="checkbox"
                      checked={form.fechaHistoria}
                      onChange={(event) => setForm((prev) => ({ ...prev, fechaHistoria: event.target.checked }))}
                    />
                    <span>Fecha história ao selecionar</span>
                  </label>
                  <label className="checkbox">
                    <input
                      type="checkbox"
                      checked={form.ativo}
                      onChange={(event) => setForm((prev) => ({ ...prev, ativo: event.target.checked }))}
                    />
                    <span>Ativo</span>
                  </label>
                </div>
                {modalError && <p className="danger-text">{modalError}</p>}
                <div className="modal-actions">
                  <button type="button" className="ghost" onClick={closeModal}>
                    Cancelar
                  </button>
                  <button type="submit" className="primary" disabled={saving}>
                    {form.id ? "Salvar alterações" : "Adicionar status"}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

type HistoriaTipoForm = Omit<CreateHistoriaTipoConfigRequest, "ordem"> & {
  ordem: string;
  id?: number | null;
};

const HistoriaTiposTab = ({ empresaId }: ConfigTabProps) => {
  const initialForm: HistoriaTipoForm = {
    id: null,
    nome: "",
    descricao: "",
    ordem: "1",
    ativo: true,
  };
  const [form, setForm] = useState(initialForm);
  const [saving, setSaving] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);
  const [modalError, setModalError] = useState<string | null>(null);
  const [bannerMessage, setBannerMessage] = useState<BannerMessage>(null);

  const { items, setItems, loading, error, load } = useAsyncList<HistoriaTipoConfigDto>(
    useCallback(() => HistoriaTipoService.getAll(empresaId), [empresaId])
  );

  useEffect(() => {
    load();
  }, [load]);

  useBodyScrollLock(modalOpen);

  const resetForm = () => setForm(initialForm);

  const closeModal = () => {
    setModalOpen(false);
    setModalError(null);
    resetForm();
  };

  const openCreate = () => {
    resetForm();
    setModalOpen(true);
  };

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setSaving(true);
    setModalError(null);
    try {
      const payload: CreateHistoriaTipoConfigRequest = {
        nome: form.nome,
        descricao: form.descricao,
        ordem: form.ordem.trim() === "" ? 0 : Number(form.ordem),
        ativo: form.ativo,
      };
      if (form.id) {
        const updated = await HistoriaTipoService.update(form.id, payload, empresaId);
        setItems((prev) => prev.map((item) => (item.id === updated.id ? updated : item)));
        setBannerMessage({ type: "info", text: "Tipo atualizado com sucesso." });
      } else {
        const created = await HistoriaTipoService.create(payload, empresaId);
        setItems((prev) => [...prev, created]);
        setBannerMessage({ type: "info", text: "Tipo criado com sucesso." });
      }
      closeModal();
    } catch (err) {
      console.error(err);
      setModalError("Não foi possível salvar o tipo.");
    } finally {
      setSaving(false);
    }
  };

  const handleEdit = (tipo: HistoriaTipoConfigDto) => {
    setForm({
      id: tipo.id,
      nome: tipo.nome,
      descricao: tipo.descricao ?? "",
      ordem: String(tipo.ordem),
      ativo: tipo.ativo,
    });
    setModalOpen(true);
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm("Excluir este tipo?")) {
      return;
    }
    try {
      await HistoriaTipoService.remove(id, empresaId);
      setItems((prev) => prev.filter((item) => item.id !== id));
      setBannerMessage({ type: "info", text: "Tipo removido com sucesso." });
    } catch (err) {
      console.error(err);
      setBannerMessage({ type: "error", text: "Não foi possível excluir o tipo." });
    }
  };

  return (
    <div className="card stack">
      <div className="filters-bar">
        <div className="filters-actions">
          <button type="button" className="primary" onClick={openCreate}>
            + Novo tipo
          </button>
        </div>
        {bannerMessage && (
          <p className={bannerMessage.type === "error" ? "danger-text" : "info-text"}>{bannerMessage.text}</p>
        )}
      </div>

      <div className="table-wrapper">
        {loading && <p>Carregando tipos...</p>}
        {error && <p className="danger-text">{error}</p>}
        {!loading && !error && (
          <table>
            <thead>
              <tr>
                <th>Nome</th>
                <th>Descrição</th>
                <th>Ordem</th>
                <th>Ativo</th>
                <th>Ações</th>
              </tr>
            </thead>
            <tbody>
              {items.length === 0 && (
                <tr>
                  <td colSpan={5}>Nenhum tipo cadastrado.</td>
                </tr>
              )}
              {items
                .slice()
                .sort((a, b) => a.ordem - b.ordem || a.id - b.id)
                .map((tipo) => (
                  <tr key={tipo.id}>
                    <td>{tipo.nome}</td>
                    <td>{tipo.descricao || "—"}</td>
                    <td>{tipo.ordem}</td>
                    <td>{tipo.ativo ? "Sim" : "Não"}</td>
                    <td className="table-actions">
                      <button type="button" className="ghost" onClick={() => handleEdit(tipo)}>
                        Editar
                      </button>
                      <button type="button" className="danger ghost" onClick={() => handleDelete(tipo.id)}>
                        Excluir
                      </button>
                    </td>
                  </tr>
                ))}
            </tbody>
          </table>
        )}
      </div>

      {modalOpen && (
        <div className="modal-overlay" onClick={closeModal}>
          <div className="modal small" onClick={(event) => event.stopPropagation()}>
            <div className="modal-header">
              <div>
                <p className="modal-eyebrow">Tipo de história</p>
                <h3>{form.id ? "Editar tipo" : "Novo tipo"}</h3>
              </div>
              <button type="button" className="ghost" onClick={closeModal}>
                Fechar
              </button>
            </div>
            <div className="modal-content">
              <form className="modal-form" onSubmit={handleSubmit}>
                <div className="modal-inline-fields">
                  <div className="form-field">
                    <label htmlFor="hist-tipo-nome">Nome</label>
                    <input
                      id="hist-tipo-nome"
                      type="text"
                      value={form.nome}
                      onChange={(event) => setForm((prev) => ({ ...prev, nome: event.target.value }))}
                      required
                    />
                  </div>
                  <div className="form-field">
                    <label htmlFor="hist-tipo-descricao">Descrição</label>
                    <input
                      id="hist-tipo-descricao"
                      type="text"
                      value={form.descricao ?? ""}
                      onChange={(event) => setForm((prev) => ({ ...prev, descricao: event.target.value }))}
                    />
                  </div>
                </div>
                <div className="modal-inline-fields">
                  <div className="form-field">
                    <label htmlFor="hist-tipo-ordem">Ordem</label>
                    <input
                      id="hist-tipo-ordem"
                      type="number"
                      value={form.ordem}
                      min={0}
                      onChange={(event) => setForm((prev) => ({ ...prev, ordem: event.target.value }))}
                    />
                  </div>
                  <div className="form-field">
                    <label htmlFor="hist-tipo-ativo">Situação</label>
                    <div className="modal-checkbox-row single">
                      <label className="checkbox">
                        <input
                          id="hist-tipo-ativo"
                          type="checkbox"
                          checked={form.ativo}
                          onChange={(event) => setForm((prev) => ({ ...prev, ativo: event.target.checked }))}
                        />
                        <span>Registro ativo</span>
                      </label>
                    </div>
                  </div>
                </div>
                {modalError && <p className="danger-text">{modalError}</p>}
                <div className="modal-actions">
                  <button type="button" className="ghost" onClick={closeModal}>
                    Cancelar
                  </button>
                  <button type="submit" className="primary" disabled={saving}>
                    {form.id ? "Salvar alterações" : "Adicionar tipo"}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

type TicketTipoForm = Omit<CreateTicketTipoConfigRequest, "ordem"> & {
  ordem: string;
  id?: number | null;
};

const TicketTiposTab = ({ empresaId }: ConfigTabProps) => {
  const initialForm: TicketTipoForm = {
    id: null,
    nome: "",
    descricao: "",
    ordem: "1",
    ativo: true,
  };
  const [form, setForm] = useState(initialForm);
  const [saving, setSaving] = useState(false);
  const [modalOpen, setModalOpen] = useState(false);
  const [modalError, setModalError] = useState<string | null>(null);
  const [bannerMessage, setBannerMessage] = useState<BannerMessage>(null);

  const { items, setItems, loading, error, load } = useAsyncList<TicketTipoConfigDto>(
    useCallback(() => TicketTipoService.getAll(empresaId), [empresaId])
  );

  useEffect(() => {
    load();
  }, [load]);

  useBodyScrollLock(modalOpen);

  const resetForm = () => setForm(initialForm);

  const closeModal = () => {
    setModalOpen(false);
    setModalError(null);
    resetForm();
  };

  const openCreate = () => {
    resetForm();
    setModalOpen(true);
  };

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setSaving(true);
    setModalError(null);
    try {
      const payload: CreateTicketTipoConfigRequest = {
        nome: form.nome,
        descricao: form.descricao,
        ordem: form.ordem.trim() === "" ? 0 : Number(form.ordem),
        ativo: form.ativo,
      };
      if (form.id) {
        const updated = await TicketTipoService.update(form.id, payload, empresaId);
        setItems((prev) => prev.map((item) => (item.id === updated.id ? updated : item)));
        setBannerMessage({ type: "info", text: "Tipo de ticket atualizado com sucesso." });
      } else {
        const created = await TicketTipoService.create(payload, empresaId);
        setItems((prev) => [...prev, created]);
        setBannerMessage({ type: "info", text: "Tipo de ticket criado com sucesso." });
      }
      closeModal();
    } catch (err) {
      console.error(err);
      setModalError("Não foi possível salvar o tipo de ticket.");
    } finally {
      setSaving(false);
    }
  };

  const handleEdit = (tipo: TicketTipoConfigDto) => {
    setForm({
      id: tipo.id,
      nome: tipo.nome,
      descricao: tipo.descricao ?? "",
      ordem: String(tipo.ordem),
      ativo: tipo.ativo,
    });
    setModalOpen(true);
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm("Excluir este tipo de ticket?")) {
      return;
    }
    try {
      await TicketTipoService.remove(id, empresaId);
      setItems((prev) => prev.filter((item) => item.id !== id));
      setBannerMessage({ type: "info", text: "Tipo de ticket removido." });
    } catch (err) {
      console.error(err);
      setBannerMessage({ type: "error", text: "Não foi possível excluir o tipo de ticket." });
    }
  };

  return (
    <div className="card stack">
      <div className="filters-bar">
        <div className="filters-actions">
          <button type="button" className="primary" onClick={openCreate}>
            + Novo tipo de ticket
          </button>
        </div>
        {bannerMessage && (
          <p className={bannerMessage.type === "error" ? "danger-text" : "info-text"}>{bannerMessage.text}</p>
        )}
      </div>

      <div className="table-wrapper">
        {loading && <p>Carregando tipos...</p>}
        {error && <p className="danger-text">{error}</p>}
        {!loading && !error && (
          <table>
            <thead>
              <tr>
                <th>Nome</th>
                <th>Descrição</th>
                <th>Ordem</th>
                <th>Ativo</th>
                <th>Ações</th>
              </tr>
            </thead>
            <tbody>
              {items.length === 0 && (
                <tr>
                  <td colSpan={5}>Nenhum tipo cadastrado.</td>
                </tr>
              )}
              {items
                .slice()
                .sort((a, b) => a.ordem - b.ordem || a.id - b.id)
                .map((tipo) => (
                  <tr key={tipo.id}>
                    <td>{tipo.nome}</td>
                    <td>{tipo.descricao || "—"}</td>
                    <td>{tipo.ordem}</td>
                    <td>{tipo.ativo ? "Sim" : "Não"}</td>
                    <td className="table-actions">
                      <button type="button" className="ghost" onClick={() => handleEdit(tipo)}>
                        Editar
                      </button>
                      <button type="button" className="danger ghost" onClick={() => handleDelete(tipo.id)}>
                        Excluir
                      </button>
                    </td>
                  </tr>
                ))}
            </tbody>
          </table>
        )}
      </div>

      {modalOpen && (
        <div className="modal-overlay" onClick={closeModal}>
          <div className="modal small" onClick={(event) => event.stopPropagation()}>
            <div className="modal-header">
              <div>
                <p className="modal-eyebrow">Tipo de ticket</p>
                <h3>{form.id ? "Editar tipo" : "Novo tipo"}</h3>
              </div>
              <button type="button" className="ghost" onClick={closeModal}>
                Fechar
              </button>
            </div>
            <div className="modal-content">
              <form className="modal-form" onSubmit={handleSubmit}>
                <div className="modal-inline-fields">
                  <div className="form-field">
                    <label htmlFor="ticket-tipo-nome">Nome</label>
                    <input
                      id="ticket-tipo-nome"
                      type="text"
                      value={form.nome}
                      onChange={(event) => setForm((prev) => ({ ...prev, nome: event.target.value }))}
                      required
                    />
                  </div>
                  <div className="form-field">
                    <label htmlFor="ticket-tipo-descricao">Descrição</label>
                    <input
                      id="ticket-tipo-descricao"
                      type="text"
                      value={form.descricao ?? ""}
                      onChange={(event) => setForm((prev) => ({ ...prev, descricao: event.target.value }))}
                    />
                  </div>
                </div>
                <div className="modal-inline-fields">
                  <div className="form-field">
                    <label htmlFor="ticket-tipo-ordem">Ordem</label>
                    <input
                      id="ticket-tipo-ordem"
                      type="number"
                      min={0}
                      value={form.ordem}
                      onChange={(event) => setForm((prev) => ({ ...prev, ordem: event.target.value }))}
                    />
                  </div>
                  <div className="form-field">
                    <label htmlFor="ticket-tipo-ativo">Situação</label>
                    <div className="modal-checkbox-row single">
                      <label className="checkbox">
                        <input
                          id="ticket-tipo-ativo"
                          type="checkbox"
                          checked={form.ativo}
                          onChange={(event) => setForm((prev) => ({ ...prev, ativo: event.target.checked }))}
                        />
                        <span>Registro ativo</span>
                      </label>
                    </div>
                  </div>
                </div>
                {modalError && <p className="danger-text">{modalError}</p>}
                <div className="modal-actions">
                  <button type="button" className="ghost" onClick={closeModal}>
                    Cancelar
                  </button>
                  <button type="submit" className="primary" disabled={saving}>
                    {form.id ? "Salvar alterações" : "Adicionar tipo"}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

const ConfiguracoesCadastrosPage = () => {
  const { selectedEmpresaId } = useAuth();
  const [activeTab, setActiveTab] = useState<TabKey>("historia-status");

  const empresaId = useMemo(() => selectedEmpresaId ?? null, [selectedEmpresaId]);

  return (
    <AppLayout
      title="Cadastros avançados"
      subtitle="Configure status e tipos utilizados em histórias e tickets"
    >
      <div className="filters-bar">
        <div className="view-toggle">
          {Object.entries(tabOptions).map(([key, label]) => (
            <button
              key={key}
              type="button"
              className={`ghost ${activeTab === key ? "active" : ""}`}
              onClick={() => setActiveTab(key as TabKey)}
            >
              {label}
            </button>
          ))}
        </div>
      </div>

      {activeTab === "historia-status" && <HistoriaStatusTab empresaId={empresaId} />}
      {activeTab === "historia-tipos" && <HistoriaTiposTab empresaId={empresaId} />}
      {activeTab === "ticket-tipos" && <TicketTiposTab empresaId={empresaId} />}
    </AppLayout>
  );
};

export default ConfiguracoesCadastrosPage;

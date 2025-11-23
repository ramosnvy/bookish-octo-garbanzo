import { useEffect, useMemo, useRef, useState } from "react";
import { useNavigate } from "react-router-dom";
import AppLayout from "../components/AppLayout";
import { CreateEmpresaRequest, EmpresaDto, UpdateEmpresaRequest } from "../models";
import { EmpresaService } from "../services/EmpresaService";
import { useAuth } from "../context/AuthContext";
import { getApiErrorMessage } from "../utils/apiError";

const initialForm: CreateEmpresaRequest = {
  nome: "",
  documento: "",
  emailContato: "",
  telefoneContato: "",
  ativo: true,
  usuarioInicial: {
    nome: "",
    email: "",
    password: "",
  },
};

const EmpresasPage = () => {
  const navigate = useNavigate();
  const { user } = useAuth();
  const [empresas, setEmpresas] = useState<EmpresaDto[]>([]);
  const [form, setForm] = useState<CreateEmpresaRequest>(initialForm);
  const [search, setSearch] = useState("");
  const [toastMessage, setToastMessage] = useState<string | null>(null);
  const toastTimeout = useRef<ReturnType<typeof setTimeout> | null>(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [isEditing, setIsEditing] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);

  const showToast = (message: string) => {
    if (!message) return;
    setToastMessage(message);
    if (toastTimeout.current) {
      clearTimeout(toastTimeout.current);
    }
    toastTimeout.current = setTimeout(() => {
      setToastMessage(null);
      toastTimeout.current = null;
    }, 3000);
  };

  useEffect(() => {
    return () => {
      if (toastTimeout.current) {
        clearTimeout(toastTimeout.current);
      }
    };
  }, []);

  useEffect(() => {
    if (user?.empresaId) {
      navigate("/", { replace: true });
      return;
    }

    EmpresaService.getAll()
      .then(setEmpresas)
      .catch(() => showToast("Não foi possível carregar as empresas."));
  }, [user?.empresaId, navigate]);

  const filteredEmpresas = useMemo(() => {
    if (!search.trim()) {
      return empresas;
    }
    const term = search.trim().toLowerCase();
    return empresas.filter(
      (empresa) => empresa.nome.toLowerCase().includes(term) || empresa.documento.toLowerCase().includes(term)
    );
  }, [empresas, search]);

  const stats = useMemo(
    () => [
      { label: "Total", value: empresas.length },
      { label: "Ativas", value: empresas.filter((empresa) => empresa.ativo).length },
    ],
    [empresas]
  );

  const handleOpenCreate = () => {
    setIsEditing(false);
    setEditingId(null);
    setForm(initialForm);
    setModalOpen(true);
  };

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    try {
      if (isEditing && editingId) {
        const payload: UpdateEmpresaRequest = {
          nome: form.nome,
          documento: form.documento,
          emailContato: form.emailContato,
          telefoneContato: form.telefoneContato,
          ativo: form.ativo,
        };
        const atualizada = await EmpresaService.update(editingId, payload);
        setEmpresas((prev) => prev.map((empresa) => (empresa.id === atualizada.id ? atualizada : empresa)));
        showToast("Empresa atualizada com sucesso.");
      } else {
        const criada = await EmpresaService.create(form);
        setEmpresas((prev) => [...prev, criada]);
        showToast("Empresa cadastrada com sucesso.");
      }
      setForm(initialForm);
      setEditingId(null);
      setIsEditing(false);
      setModalOpen(false);
    } catch (error: any) {
      const fallback = isEditing ? "Não foi possível atualizar a empresa." : "Não foi possível criar a empresa.";
      showToast(getApiErrorMessage(error, fallback));
    }
  };

  if (user?.empresaId) {
    return null;
  }

  return (
    <AppLayout title="Empresas" subtitle="Administração global do sistema">
      {toastMessage && <div className="toast alert">{toastMessage}</div>}
      <section className="filters-bar">
        <input
          className="search-input"
          placeholder="Pesquisar empresas..."
          value={search}
          onChange={(event) => setSearch(event.target.value)}
        />
        <div className="filters-actions">
          <button className="primary" onClick={handleOpenCreate}>
            + Nova empresa
          </button>
        </div>
      </section>

      <section className="stats-grid">
        {stats.map((stat) => (
          <article key={stat.label} className="card stat-card">
            <p className="muted">{stat.label}</p>
            <h3>{stat.value}</h3>
          </article>
        ))}
      </section>

      <section className="card table-wrapper">
        <table>
          <thead>
            <tr>
              <th>Empresa</th>
              <th>Contato</th>
              <th>Status</th>
              <th>Criada em</th>
              <th>Ações</th>
            </tr>
          </thead>
          <tbody>
            {filteredEmpresas.length === 0 ? (
              <tr>
                <td colSpan={5}>Nenhuma empresa encontrada.</td>
              </tr>
            ) : (
              filteredEmpresas.map((empresa) => (
                <tr key={empresa.id}>
                  <td>
                    <p className="table-title">{empresa.nome}</p>
                    <small>{empresa.documento || "Documento não informado"}</small>
                  </td>
                  <td>
                    <p>{empresa.emailContato}</p>
                    <small>{empresa.telefoneContato || "-"}</small>
                  </td>
                  <td>
                    <span className={`badge ${empresa.ativo ? "ativo" : "inativo"}`}>
                      {empresa.ativo ? "Ativa" : "Inativa"}
                    </span>
                  </td>
                  <td>{new Date(empresa.createdAt).toLocaleDateString("pt-BR")}</td>
                  <td>
                    <div className="table-actions">
                      <button
                        className="ghost"
                        onClick={() => {
                          setIsEditing(true);
                          setEditingId(empresa.id);
                          setForm({
                            nome: empresa.nome,
                            documento: empresa.documento,
                            emailContato: empresa.emailContato,
                            telefoneContato: empresa.telefoneContato,
                            ativo: empresa.ativo,
                            usuarioInicial: { nome: "", email: "", password: "" },
                          });
                          setModalOpen(true);
                        }}
                      >
                        Editar
                      </button>
                      <button
                        className="ghost"
                        onClick={async () => {
                          try {
                            const updated = await EmpresaService.update(empresa.id, {
                              nome: empresa.nome,
                              documento: empresa.documento,
                              emailContato: empresa.emailContato,
                              telefoneContato: empresa.telefoneContato,
                              ativo: !empresa.ativo,
                            });
                            setEmpresas((prev) => prev.map((item) => (item.id === updated.id ? updated : item)));
                            showToast(updated.ativo ? "Empresa ativada." : "Empresa desativada.");
                          } catch {
                            showToast("Não foi possível atualizar o status da empresa.");
                          }
                        }}
                      >
                        {empresa.ativo ? "Desativar" : "Ativar"}
                      </button>
                    </div>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </section>

      {modalOpen && (
        <div className="modal-overlay" onClick={() => setModalOpen(false)}>
          <div className="modal" onClick={(event) => event.stopPropagation()}>
            <header className="modal-header">
              <h3>Nova empresa</h3>
              <button className="ghost" onClick={() => setModalOpen(false)}>
                Fechar
              </button>
            </header>
            <div className="modal-content">
              <form className="form-grid" onSubmit={handleSubmit}>
                <section className="form-section">
                  <div className="form-row">
                    <label className="form-field">
                      <span className="form-label">Nome</span>
                      <input value={form.nome} onChange={(event) => setForm({ ...form, nome: event.target.value })} required />
                    </label>
                    <label className="form-field">
                      <span className="form-label">Documento</span>
                      <input value={form.documento} onChange={(event) => setForm({ ...form, documento: event.target.value })} />
                    </label>
                  </div>
                  <div className="form-row">
                    <label className="form-field">
                      <span className="form-label">Email de contato</span>
                      <input
                        type="email"
                        value={form.emailContato}
                        onChange={(event) => setForm({ ...form, emailContato: event.target.value })}
                      />
                    </label>
                    <label className="form-field">
                      <span className="form-label">Telefone</span>
                      <input
                        value={form.telefoneContato}
                        onChange={(event) => setForm({ ...form, telefoneContato: event.target.value })}
                      />
                    </label>
                    <div className="form-field switch-field">
                      <span className="form-label">Status</span>
                      <label className="toggle-switch">
                        <input
                          type="checkbox"
                          checked={form.ativo}
                          onChange={(event) => setForm({ ...form, ativo: event.target.checked })}
                        />
                        <span className="slider" />
                      </label>
                      <span className={`toggle-status ${form.ativo ? "active" : "inactive"}`}>
                        {form.ativo ? "Ativa" : "Inativa"}
                      </span>
                    </div>
                  </div>
                </section>
                {!isEditing && (
                  <section className="form-section">
                    <p className="form-section-title">Usuário inicial</p>
                    <div className="form-row">
                      <label className="form-field">
                        <span className="form-label">Nome</span>
                        <input
                          value={form.usuarioInicial.nome}
                          onChange={(event) => setForm({ ...form, usuarioInicial: { ...form.usuarioInicial, nome: event.target.value } })}
                          required
                        />
                      </label>
                      <label className="form-field">
                        <span className="form-label">Email</span>
                        <input
                          type="email"
                          value={form.usuarioInicial.email}
                          onChange={(event) => setForm({ ...form, usuarioInicial: { ...form.usuarioInicial, email: event.target.value } })}
                          required
                        />
                      </label>
                    </div>
                    <div className="form-row">
                      <label className="form-field form-field--full">
                        <span className="form-label">Senha inicial</span>
                        <input
                          type="password"
                          value={form.usuarioInicial.password}
                          onChange={(event) =>
                            setForm({ ...form, usuarioInicial: { ...form.usuarioInicial, password: event.target.value } })
                          }
                          required
                        />
                      </label>
                    </div>
                  </section>
                )}
                <div className="modal-actions">
                  <button
                    type="button"
                    className="ghost"
                    onClick={() => {
                      setModalOpen(false);
                      setIsEditing(false);
                      setEditingId(null);
                      setForm(initialForm);
                    }}
                  >
                    Cancelar
                  </button>
                  <button type="submit" className="primary">
                    {isEditing ? "Salvar alterações" : "Salvar"}
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

export default EmpresasPage;

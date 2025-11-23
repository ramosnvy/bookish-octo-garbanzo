import { useEffect, useMemo, useRef, useState } from "react";
import AppLayout from "../components/AppLayout";
import { CreateUserRequest, UpdateUserRequest, UserDto, EmpresaDto } from "../models";
import { UserService } from "../services/UserService";
import { EmpresaService } from "../services/EmpresaService";
import { useAuth } from "../context/AuthContext";
import { getApiErrorMessage } from "../utils/apiError";

const roleOptions = [
  { value: "Admin", label: "Administrador" },
  { value: "Manager", label: "Gestor" },
  { value: "Employee", label: "Funcionário" },
];

const roleLabelMap: Record<string, string> = roleOptions.reduce((acc, option) => {
  acc[option.value.toLowerCase()] = option.label;
  return acc;
}, {} as Record<string, string>);

const roleNumberMap: Record<number, string> = {
  1: "Admin",
  2: "Manager",
  3: "Employee",
};

const normalizeRoleValue = (role: string | number | undefined): string => {
  if (typeof role === "number") {
    return roleNumberMap[role] ?? "Employee";
  }
  if (!role) {
    return "Employee";
  }
  return role;
};

const formatRoleLabel = (role: string | number | undefined): string => {
  const normalized = normalizeRoleValue(role).toLowerCase();
  return roleLabelMap[normalized] ?? normalized;
};

type UserFormState = {
  nome: string;
  email: string;
  password: string;
  role: string;
  empresaId: number | "";
};

const createInitialForm = (): UserFormState => ({
  nome: "",
  email: "",
  password: "",
  role: "Employee",
  empresaId: "",
});

const UsersPage = () => {
  const { user: currentUser, selectedEmpresaId } = useAuth();
  const isGlobalAdmin = currentUser?.role?.toLowerCase() === "admin" && !currentUser?.empresaId;
  const empresaFiltroAtual = currentUser?.empresaId ?? selectedEmpresaId ?? null;
  const [users, setUsers] = useState<UserDto[]>([]);
  const [search, setSearch] = useState("");
  const [roleFilter, setRoleFilter] = useState<string>("all");
  const [modalOpen, setModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [form, setForm] = useState<UserFormState>(createInitialForm());
  const [companies, setCompanies] = useState<EmpresaDto[]>([]);
  const [toastMessage, setToastMessage] = useState<string | null>(null);
  const toastTimeout = useRef<ReturnType<typeof setTimeout> | null>(null);

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
    const empresaFiltro = currentUser?.empresaId ?? selectedEmpresaId;
    UserService.getAll(empresaFiltro)
      .then(setUsers)
      .catch(() => showToast("Não foi possível carregar os usuários."));
  }, [currentUser?.empresaId, selectedEmpresaId]);

  useEffect(() => {
    if (isGlobalAdmin) {
      EmpresaService.getAll()
        .then(setCompanies)
        .catch(() => showToast("Não foi possível carregar as empresas."));
    }
  }, [isGlobalAdmin]);

  const filteredUsers = useMemo(() => {
    return users.filter((user) => {
      const normalizedRole = normalizeRoleValue(user.role);
      if (roleFilter !== "all" && normalizedRole !== roleFilter) {
        return false;
      }
      if (empresaFiltroAtual) {
        if (user.empresaId !== empresaFiltroAtual) {
          return false;
        }
      }
      if (search.trim()) {
        const term = search.trim().toLowerCase();
        return [user.nome, user.email].some((field) => field.toLowerCase().includes(term));
      }
      return true;
    });
  }, [users, search, roleFilter, empresaFiltroAtual]);

  const stats = useMemo(
    () => [
      { label: "Total", value: users.length },
      { label: "Administradores", value: users.filter((user) => normalizeRoleValue(user.role) === "Admin").length },
      { label: "Gestores", value: users.filter((user) => normalizeRoleValue(user.role) === "Manager").length },
      { label: "Funcionários", value: users.filter((user) => normalizeRoleValue(user.role) === "Employee").length },
    ],
    [users]
  );

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();

    try {
      if (editingId) {
        const payload: UpdateUserRequest = {
          nome: form.nome,
          email: form.email,
          role: form.role,
          empresaId: form.empresaId ? Number(form.empresaId) : currentUser?.empresaId ?? selectedEmpresaId ?? null,
        };
        const atualizado = await UserService.update(editingId, payload);
      setUsers((prev) => prev.map((user) => (user.id === atualizado.id ? atualizado : user)));
      showToast("Usuário atualizado com sucesso.");
    } else {
        if (!form.password.trim()) {
          showToast("Informe uma senha para o novo usuário.");
          return;
        }
        const payload: CreateUserRequest = {
          nome: form.nome,
          email: form.email,
          role: form.role,
          password: form.password,
          empresaId: form.empresaId ? Number(form.empresaId) : currentUser?.empresaId ?? selectedEmpresaId ?? null,
        };
        const created = await UserService.create(payload);
        setUsers((prev) => [...prev, created]);
        showToast("Usuário criado com sucesso.");
      }

      setForm(createInitialForm());
      setEditingId(null);
      setModalOpen(false);
    } catch (error: any) {
      showToast(getApiErrorMessage(error, "Não foi possível salvar o usuário."));
    }
  };

  const handleOpenCreate = () => {
    setForm({ ...createInitialForm(), empresaId: currentUser?.empresaId ?? selectedEmpresaId ?? "" });
    setEditingId(null);
    setModalOpen(true);
  };

  const handleOpenEdit = (user: UserDto) => {
    setEditingId(user.id);
    setForm({
      nome: user.nome,
      email: user.email,
      password: "",
      role: normalizeRoleValue(user.role),
      empresaId: user.empresaId ?? currentUser?.empresaId ?? selectedEmpresaId ?? "",
    });
    setModalOpen(true);
  };

  const handleDelete = async (id: number) => {
    if (!window.confirm("Deseja remover este usuário?")) {
      return;
    }
    try {
      await UserService.remove(id);
      setUsers((prev) => prev.filter((user) => user.id !== id));
      showToast("Usuário removido com sucesso.");
    } catch (error: any) {
      showToast(getApiErrorMessage(error, "Erro ao remover o usuário."));
    }
  };

  return (
    <AppLayout title="Usuários" subtitle="Controle de acessos e perfis do sistema">
      {toastMessage && <div className="toast alert">{toastMessage}</div>}
      <section className="filters-bar">
        <input
          className="search-input"
          placeholder="Pesquisar por nome ou email"
          value={search}
          onChange={(event) => setSearch(event.target.value)}
        />
        <div className="filters-group compact">
          <div className="select-wrapper">
            <select value={roleFilter} onChange={(event) => setRoleFilter(event.target.value)}>
              <option value="all">Todos os perfis</option>
              {roleOptions.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </div>
        </div>
        <div className="filters-actions">
          <button className="primary" onClick={handleOpenCreate}>
            + Novo usuário
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
              <th>Usuário</th>
              <th>Contato</th>
              <th>Perfil</th>
              <th>Criado em</th>
              <th>Ações</th>
            </tr>
          </thead>
          <tbody>
            {filteredUsers.length === 0 ? (
              <tr>
                <td colSpan={5}>Nenhum usuário encontrado.</td>
              </tr>
            ) : (
              filteredUsers.map((user) => (
                <tr key={user.id}>
                  <td>
                    <p className="table-title">{user.nome}</p>
                    <small>ID #{user.id}</small>
                  </td>
                  <td>
                    <p>{user.email}</p>
                  </td>
                  <td>{formatRoleLabel(user.role)}</td>
                  <td>
                    {new Date(user.createdAt).toLocaleDateString("pt-BR", {
                      day: "2-digit",
                      month: "short",
                      year: "numeric",
                    })}
                  </td>
                  <td>
                    <div className="table-actions">
                      <button className="ghost" onClick={() => handleOpenEdit(user)}>
                        Editar
                      </button>
                      <button className="ghost" onClick={() => handleDelete(user.id)}>
                        Remover
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
              <h3>{editingId ? "Editar usuário" : "Novo usuário"}</h3>
              <button className="ghost" onClick={() => setModalOpen(false)}>
                Fechar
              </button>
            </header>
            <div className="modal-content">
              <form className="form-grid" onSubmit={handleSubmit}>
                <section className="form-section">
            <div className="form-row">
              <label className="form-field">
                <span className="form-label">Nome completo</span>
                <input value={form.nome} onChange={(event) => setForm({ ...form, nome: event.target.value })} required />
              </label>
                    <label className="form-field">
                      <span className="form-label">Email</span>
                      <input
                        type="email"
                        value={form.email}
                        onChange={(event) => setForm({ ...form, email: event.target.value })}
                        required
                      />
                    </label>
                  </div>
              <div className="form-row">
                <label className="form-field">
                  <span className="form-label">Perfil</span>
                  <div className="select-wrapper">
                    <select value={form.role} onChange={(event) => setForm({ ...form, role: event.target.value })}>
                      {roleOptions.map((option) => (
                        <option key={option.value} value={option.value}>
                          {option.label}
                        </option>
                      ))}
                    </select>
                  </div>
                </label>
                {isGlobalAdmin && (
                  <label className="form-field">
                    <span className="form-label">Empresa</span>
                    <div className="select-wrapper">
                      <select
                        value={form.empresaId}
                        onChange={(event) => setForm({ ...form, empresaId: event.target.value ? Number(event.target.value) : "" })}
                      >
                        <option value="">Sem vínculo</option>
                        {companies.map((empresa) => (
                          <option key={empresa.id} value={empresa.id}>
                            {empresa.nome}
                          </option>
                        ))}
                      </select>
                    </div>
                  </label>
                )}
                {!editingId && (
                  <label className="form-field">
                    <span className="form-label">Senha inicial</span>
                    <input
                      type="password"
                      value={form.password}
                      onChange={(event) => setForm({ ...form, password: event.target.value })}
                      required
                    />
                  </label>
                )}
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

export default UsersPage;

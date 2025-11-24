import { useEffect, useMemo, useState } from "react";
import AppLayout from "../components/AppLayout";
import { FornecedorCategoriaDto } from "../models";
import { FornecedorCategoriaService } from "../services/FornecedorCategoriaService";
import { useAuth } from "../context/AuthContext";

type CategoriaForm = Omit<FornecedorCategoriaDto, "id">;

const emptyForm: CategoriaForm = {
  nome: "",
  ativo: true,
};

const FornecedorCategoriasPage = () => {
  const { selectedEmpresaId, user } = useAuth();
  const [categorias, setCategorias] = useState<FornecedorCategoriaDto[]>([]);
  const [search, setSearch] = useState("");
  const [form, setForm] = useState<CategoriaForm>(emptyForm);
  const [modalOpen, setModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [loading, setLoading] = useState(true);
  const normalizedRole = user?.role?.toLowerCase();
  const isGlobalAdmin = normalizedRole === "admin" && !user?.empresaId;

  useEffect(() => {
    const load = async () => {
      setLoading(true);
      try {
        const data = await FornecedorCategoriaService.getAll(selectedEmpresaId);
        setCategorias(data);
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [selectedEmpresaId]);

  const filteredCategorias = useMemo(() => {
    if (!search) return categorias;
    const normalized = search.toLowerCase();
    return categorias.filter((c) => c.nome.toLowerCase().includes(normalized));
  }, [categorias, search]);

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    if (!form.nome.trim()) return;
    if (isGlobalAdmin) return;

    if (editingId) {
      const updated = await FornecedorCategoriaService.update(editingId, form);
      setCategorias((prev) => prev.map((c) => (c.id === updated.id ? updated : c)));
    } else {
      const created = await FornecedorCategoriaService.create(form);
      setCategorias((prev) => [...prev, created]);
    }

    setForm(emptyForm);
    setEditingId(null);
    setModalOpen(false);
  };

  const handleOpenCreate = () => {
    if (isGlobalAdmin) return;
    setForm(emptyForm);
    setEditingId(null);
    setModalOpen(true);
  };

  const handleOpenEdit = (categoria: FornecedorCategoriaDto) => {
    if (isGlobalAdmin) return;
    setEditingId(categoria.id);
    setForm({
      nome: categoria.nome,
      ativo: categoria.ativo,
    });
    setModalOpen(true);
  };

  const handleDelete = async (id: number) => {
    if (isGlobalAdmin) return;
    if (!window.confirm("Tem certeza que deseja excluir esta categoria?")) {
      return;
    }
    await FornecedorCategoriaService.delete(id);
    setCategorias((prev) => prev.filter((c) => c.id !== id));
  };

  return (
    <AppLayout title="Categorias de Fornecedores">
      {isGlobalAdmin && (
        <div className="toast info">Administradores globais possuem acesso somente leitura nesta tela.</div>
      )}
      <section className="filters-bar">
        <input
          className="search-input"
          placeholder="Pesquisar categorias..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        {!isGlobalAdmin && (
          <button className="primary" onClick={handleOpenCreate}>
            + Nova categoria
          </button>
        )}
      </section>

      <section className="card table-wrapper">
        <table>
          <thead>
            <tr>
              <th>Nome</th>
              <th>Status</th>
              {!isGlobalAdmin && <th>Ações</th>}
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr>
                <td colSpan={4}>Carregando...</td>
              </tr>
            ) : filteredCategorias.length === 0 ? (
              <tr>
                <td colSpan={4}>Nenhuma categoria encontrada.</td>
              </tr>
            ) : (
              filteredCategorias.map((categoria) => (
                <tr key={categoria.id}>
                  <td className="table-title">{categoria.nome}</td>
                  <td>
                    <span className={`badge ${categoria.ativo ? "ativo" : "inativo"}`}>
                      {categoria.ativo ? "Ativa" : "Inativa"}
                    </span>
                  </td>
                  {!isGlobalAdmin && (
                    <td>
                      <div className="table-actions">
                        <button
                          className="ghost primary icon-only"
                          onClick={() => handleOpenEdit(categoria)}
                          aria-label={`Editar categoria ${categoria.nome}`}
                          title="Editar categoria"
                        >
                          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
                            <path
                              d="M7 17L5 19V15L15.5 4.5C16.0523 3.94772 16.9477 3.94772 17.5 4.5L19.5 6.5C20.0523 7.05228 20.0523 7.94772 19.5 8.5L9 19H7Z"
                              stroke="currentColor"
                              strokeWidth="1.6"
                              strokeLinecap="round"
                              strokeLinejoin="round"
                            />
                          </svg>
                        </button>
                        <button
                          className="ghost icon-only danger"
                          onClick={() => handleDelete(categoria.id)}
                          aria-label={`Excluir categoria ${categoria.nome}`}
                          title="Excluir categoria"
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
              ))
            )}
          </tbody>
        </table>
      </section>

      {!isGlobalAdmin && modalOpen && (
        <div className="modal-overlay" onClick={() => setModalOpen(false)}>
          <div className="modal small" onClick={(e) => e.stopPropagation()}>
            <header className="modal-header">
              <h3>{editingId ? "Editar categoria" : "Nova categoria"}</h3>
              <button className="ghost" onClick={() => setModalOpen(false)}>
                Fechar
              </button>
            </header>
            <div className="modal-content">
              <form className="form-inline" onSubmit={handleSubmit}>
                <input
                  placeholder="Nome"
                  value={form.nome}
                  onChange={(e) => setForm({ ...form, nome: e.target.value })}
                  required
                />
                <div className="form-field switch-field">
                  <span className="form-label">Status da categoria</span>
                  <label className="toggle-switch">
                    <input
                      type="checkbox"
                      checked={form.ativo}
                      onChange={(e) => setForm({ ...form, ativo: e.target.checked })}
                    />
                    <span className="slider" />
                  </label>
                  <span className={`toggle-status ${form.ativo ? "active" : "inactive"}`}>
                    {form.ativo ? "Categoria ativa" : "Categoria inativa"}
                  </span>
                </div>
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

export default FornecedorCategoriasPage;

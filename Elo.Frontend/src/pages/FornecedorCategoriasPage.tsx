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
                        <button className="ghost" onClick={() => handleOpenEdit(categoria)}>
                          Editar
                        </button>
                        <button className="ghost" onClick={() => handleDelete(categoria.id)}>
                          Excluir
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

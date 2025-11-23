import { useEffect, useMemo, useRef, useState } from "react";
import AppLayout from "../components/AppLayout";
import { CreateProdutoRequest, ProdutoDto, FornecedorDto, ProdutoModuloDto, ProdutoModuloInput } from "../models";
import { ProdutoService } from "../services/ProdutoService";
import { FornecedorService } from "../services/FornecedorService";
import { useAuth } from "../context/AuthContext";
import { getApiErrorMessage } from "../utils/apiError";

const currencyFormatter = new Intl.NumberFormat("pt-BR", {
  style: "currency",
  currency: "BRL",
});

type ProdutoModuloForm = {
  nome: string;
  descricao: string;
  valorAdicional: string;
  custoAdicional: string;
  ativo: boolean;
};

type ProdutoFormState = Omit<CreateProdutoRequest, "valorCusto" | "valorRevenda" | "fornecedorId" | "modulos"> & {
  valorCusto: string;
  valorRevenda: string;
  fornecedorId: string;
  modulos: ProdutoModuloForm[];
};

const emptyModulo: ProdutoModuloForm = {
  nome: "",
  descricao: "",
  valorAdicional: "",
  custoAdicional: "",
  ativo: true,
};

const createInitialForm = (): ProdutoFormState => ({
  nome: "",
  descricao: "",
  valorCusto: "",
  valorRevenda: "",
  ativo: true,
  fornecedorId: "",
  modulos: [{ ...emptyModulo }],
});

const initialForm: ProdutoFormState = createInitialForm();

const mapModuloDtoToForm = (modulo: ProdutoModuloDto): ProdutoModuloForm => ({
  nome: modulo.nome,
  descricao: modulo.descricao,
  valorAdicional: String(modulo.valorAdicional),
  custoAdicional: String(modulo.custoAdicional),
  ativo: modulo.ativo,
});

const ProdutosPage = () => {
  const { selectedEmpresaId, user } = useAuth();
  const [produtos, setProdutos] = useState<ProdutoDto[]>([]);
  const [form, setForm] = useState<ProdutoFormState>(initialForm);
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState<"all" | "ativo" | "inativo">("all");
  const [minPrice, setMinPrice] = useState("");
  const [maxPrice, setMaxPrice] = useState("");
  const [modalOpen, setModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [fornecedores, setFornecedores] = useState<FornecedorDto[]>([]);
  const [modulesViewer, setModulesViewer] = useState<ProdutoDto | null>(null);
  const [toastMessage, setToastMessage] = useState<string | null>(null);
  const toastTimeout = useRef<ReturnType<typeof setTimeout> | null>(null);
  const normalizedRole = user?.role?.toLowerCase();
  const isGlobalAdmin = normalizedRole === "admin" && !user?.empresaId;

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
    ProdutoService.getAll(selectedEmpresaId)
      .then(setProdutos)
      .catch((error) => {
        showToast(getApiErrorMessage(error, "Não foi possível carregar os produtos."));
      });
  }, [selectedEmpresaId]);

  useEffect(() => {
    FornecedorService.getAll(selectedEmpresaId)
      .then(setFornecedores)
      .catch(() => {
        setFornecedores([]);
        showToast("Não foi possível carregar os fornecedores.");
      });
  }, [selectedEmpresaId]);

  const filteredProdutos = useMemo(() => {
    let result = produtos;
    if (search.trim()) {
      const normalized = search.trim().toLowerCase();
      result = result.filter((produto) =>
        [produto.nome, produto.descricao ?? ""].some((field) => field.toLowerCase().includes(normalized))
      );
    }
    if (statusFilter !== "all") {
      const ativo = statusFilter === "ativo";
      result = result.filter((produto) => produto.ativo === ativo);
    }
    const min = Number(minPrice);
    const max = Number(maxPrice);
    if (!Number.isNaN(min) && minPrice !== "") {
      result = result.filter((produto) => produto.valorRevenda >= min);
    }
    if (!Number.isNaN(max) && maxPrice !== "") {
      result = result.filter((produto) => produto.valorRevenda <= max);
    }
    return result;
  }, [produtos, search, statusFilter, minPrice, maxPrice]);

  const stats = useMemo(
    () => [
      { label: "Total", value: produtos.length },
      { label: "Ativos", value: produtos.filter((produto) => produto.ativo).length },
      { label: "Inativos", value: produtos.filter((produto) => !produto.ativo).length },
      {
        label: "Markup média",
        value:
          produtos.length === 0
            ? "-"
            : `${(
                produtos.reduce((total, produto) => total + Number(produto.margemLucro ?? 0), 0) / produtos.length
              ).toFixed(2)}%`,
      },
    ],
    [produtos]
  );

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    if (isGlobalAdmin) {
      showToast("Administradores globais possuem acesso somente leitura.");
      return;
    }
    const fornecedorIdValue = form.fornecedorId ? Number(form.fornecedorId) : null;
    const modulosPayload: ProdutoModuloInput[] = form.modulos
      .filter((modulo) => modulo.nome.trim().length > 0)
      .map((modulo) => ({
        nome: modulo.nome,
        descricao: modulo.descricao,
        valorAdicional: Number(modulo.valorAdicional) || 0,
        custoAdicional: Number(modulo.custoAdicional) || 0,
        ativo: modulo.ativo,
      }));
    const payload: CreateProdutoRequest = {
      nome: form.nome,
      descricao: form.descricao,
      valorCusto: Number(form.valorCusto) || 0,
      valorRevenda: Number(form.valorRevenda) || 0,
      ativo: form.ativo,
      fornecedorId: fornecedorIdValue,
      modulos: modulosPayload,
    };

    try {
      if (editingId) {
        const atualizado = await ProdutoService.update(editingId, payload);
        setProdutos((prev) => prev.map((produto) => (produto.id === atualizado.id ? atualizado : produto)));
      } else {
        const novo = await ProdutoService.create(payload);
        setProdutos((prev) => [...prev, novo]);
      }

      setForm(createInitialForm());
      setEditingId(null);
      setModalOpen(false);
    } catch (error: any) {
      showToast(getApiErrorMessage(error, "Não foi possível salvar o produto."));
    }
  };

  const handleOpenCreate = () => {
    if (isGlobalAdmin) {
      showToast("Administradores globais possuem acesso somente leitura.");
      return;
    }
    setForm(createInitialForm());
    setEditingId(null);
    setModalOpen(true);
  };

  const handleOpenEdit = (produto: ProdutoDto) => {
    if (isGlobalAdmin) {
      showToast("Administradores globais possuem acesso somente leitura.");
      return;
    }
    setEditingId(produto.id);
    setForm({
      nome: produto.nome,
      descricao: produto.descricao,
      valorCusto:
        produto.valorCusto !== undefined && produto.valorCusto !== null ? String(produto.valorCusto) : "",
      valorRevenda:
        produto.valorRevenda !== undefined && produto.valorRevenda !== null ? String(produto.valorRevenda) : "",
      ativo: produto.ativo,
      fornecedorId: produto.fornecedorId ? String(produto.fornecedorId) : "",
      modulos:
        produto.modulos && produto.modulos.length ? produto.modulos.map(mapModuloDtoToForm) : [emptyModulo],
    });
    setModalOpen(true);
  };

  const handleDelete = async (id: number) => {
    if (isGlobalAdmin) {
      showToast("Administradores globais possuem acesso somente leitura.");
      return;
    }
    if (!window.confirm("Deseja remover este produto?")) {
      return;
    }
    try {
      await ProdutoService.remove(id);
      setProdutos((prev) => prev.filter((produto) => produto.id !== id));
    } catch (error: any) {
      showToast(getApiErrorMessage(error, "Erro ao remover o produto."));
    }
  };

  const valorCustoNumber = Number(form.valorCusto);
  const valorRevendaNumber = Number(form.valorRevenda);
  const valorModulosNumber = form.modulos
    .filter((modulo) => modulo.ativo && modulo.valorAdicional !== "")
    .reduce((total, modulo) => total + (Number(modulo.valorAdicional) || 0), 0);
  const custoModulosNumber = form.modulos
    .filter((modulo) => modulo.ativo && modulo.custoAdicional !== "")
    .reduce((total, modulo) => total + (Number(modulo.custoAdicional) || 0), 0);
  const hasValores = form.valorCusto !== "" && form.valorRevenda !== "";
  const custoTotalPrevisto = hasValores ? valorCustoNumber + custoModulosNumber : null;
  const receitaTotalPrevista = hasValores ? valorRevendaNumber + valorModulosNumber : null;
  const lucroPrevisto =
    custoTotalPrevisto !== null && receitaTotalPrevista !== null
      ? receitaTotalPrevista - custoTotalPrevisto
      : null;

  const handleModuloChange = (index: number, field: keyof ProdutoModuloForm, value: string | boolean) => {
    setForm((prev) => {
      const modulos = prev.modulos.map((modulo, idx) =>
        idx === index ? { ...modulo, [field]: value } : modulo
      );
      return { ...prev, modulos };
    });
  };

  const addModulo = () => {
    setForm((prev) => ({ ...prev, modulos: [...prev.modulos, { ...emptyModulo }] }));
  };

  const removeModulo = (index: number) => {
    setForm((prev) => {
      if (prev.modulos.length === 1) {
        return prev;
      }
      return { ...prev, modulos: prev.modulos.filter((_, idx) => idx !== index) };
    });
  };

  return (
    <AppLayout title="Produtos" subtitle="Catálogo de itens comercializados">
      {toastMessage && <div className="toast alert">{toastMessage}</div>}
      {isGlobalAdmin && (
        <div className="toast info">Administradores globais possuem acesso somente leitura nesta tela.</div>
      )}
      <section className="filters-bar">
        <input
          className="search-input"
          placeholder="Pesquisar produtos..."
          value={search}
          onChange={(event) => setSearch(event.target.value)}
        />
        <div className="filters-group compact">
          <div className="select-wrapper">
            <select value={statusFilter} onChange={(event) => setStatusFilter(event.target.value as typeof statusFilter)}>
              <option value="all">Todos</option>
              <option value="ativo">Ativos</option>
              <option value="inativo">Inativos</option>
            </select>
          </div>
        </div>
        {!isGlobalAdmin && (
          <div className="filters-actions">
            <button className="primary" onClick={handleOpenCreate}>
              + Novo produto
            </button>
          </div>
        )}
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
              <th>Produto</th>
              <th>Custos</th>
              <th>Lucro</th>
              <th>Fornecedor</th>
              <th>Status</th>
              <th>Ações</th>
            </tr>
          </thead>
          <tbody>
            {filteredProdutos.length === 0 ? (
              <tr>
                <td colSpan={6}>Nenhum produto encontrado.</td>
              </tr>
            ) : (
              filteredProdutos.map((produto) => {
                const modulosAtivos = (produto.modulos ?? []).filter((modulo) => modulo.ativo);
                const totalModuloValor = modulosAtivos.reduce(
                  (total, modulo) => total + Number(modulo.valorAdicional || 0),
                  0
                );
                const totalModuloCusto = modulosAtivos.reduce(
                  (total, modulo) => total + Number(modulo.custoAdicional || 0),
                  0
                );
                const custoTotal = produto.valorCusto + totalModuloCusto;
                const receitaTotal = produto.valorRevenda + totalModuloValor;
                const lucroTotal = receitaTotal - custoTotal;
                return (
                  <tr key={produto.id}>
                    <td>
                      <p className="table-title">{produto.nome}</p>
                      <small>{produto.descricao || "Sem descrição"}</small>
                    </td>
                    <td>
                      <p>Base: {currencyFormatter.format(produto.valorCusto)}</p>
                    </td>
                    <td>
                      <p>Receita: {currencyFormatter.format(produto.valorRevenda)}</p>
                    </td>
                    <td>
                      <span className="muted">{produto.fornecedorNome || "Sem fornecedor"}</span>
                    </td>
                    <td>
                      <span className={`badge ${produto.ativo ? "ativo" : "inativo"}`}>
                        {produto.ativo ? "Ativo" : "Inativo"}
                      </span>
                    </td>
                    <td>
                      <div className="table-actions">
                        <button className="ghost" onClick={() => setModulesViewer(produto)}>
                          Módulos
                        </button>
                        {!isGlobalAdmin && (
                          <>
                            <button className="ghost" onClick={() => handleOpenEdit(produto)}>
                              Editar
                            </button>
                            <button className="ghost" onClick={() => handleDelete(produto.id)}>
                              Remover
                            </button>
                          </>
                        )}
                      </div>
                    </td>
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
              <h3>{editingId ? "Editar produto" : "Novo produto"}</h3>
              <button className="ghost" onClick={() => setModalOpen(false)}>
                Fechar
              </button>
            </header>
            <div className="modal-content">
              <form className="form-inline form-grid" onSubmit={handleSubmit}>
                <section className="form-section">
                  <p className="form-section-title">Informações básicas</p>
                  <div className="form-row">
                    <label className="form-field">
                      <span className="form-label">Nome do produto</span>
                      <input
                        placeholder="Nome"
                        value={form.nome}
                        onChange={(event) => setForm({ ...form, nome: event.target.value })}
                        required
                      />
                    </label>
                    <label className="form-field">
                      <span className="form-label">Fornecedor</span>
                      <div className="select-wrapper">
                        <select
                          value={form.fornecedorId}
                          onChange={(event) => setForm({ ...form, fornecedorId: event.target.value })}
                          disabled={!fornecedores.length}
                        >
                          <option value="">Selecione um fornecedor</option>
                          {fornecedores.map((fornecedor) => (
                            <option key={fornecedor.id} value={fornecedor.id}>
                              {fornecedor.nome}
                            </option>
                          ))}
                        </select>
                      </div>
                    </label>
                    <label className="form-field form-field--full">
                      <span className="form-label">Descrição</span>
                      <textarea
                        className="textarea"
                        rows={3}
                        placeholder="Descreva em poucas palavras"
                        value={form.descricao}
                        onChange={(event) => setForm({ ...form, descricao: event.target.value })}
                      />
                    </label>
                  </div>
                </section>

                <section className="form-section">
                  <p className="form-section-title">Precificação</p>
                  <div className="form-row">
                    <label className="form-field">
                      <span className="form-label">Valor de custo</span>
                      <input
                        type="number"
                        min="0"
                        step="0.01"
                        placeholder="0,00"
                        value={form.valorCusto}
                        onChange={(event) => setForm({ ...form, valorCusto: event.target.value })}
                        required
                      />
                    </label>
                    <label className="form-field">
                      <span className="form-label">Valor de revenda</span>
                      <input
                        type="number"
                        min="0"
                        step="0.01"
                        placeholder="0,00"
                        value={form.valorRevenda}
                        onChange={(event) => setForm({ ...form, valorRevenda: event.target.value })}
                        required
                      />
                    </label>
                  </div>
                </section>

                <section className="form-section">
                  <p className="form-section-title">Disponibilidade</p>
                  <div className="form-row">
                    <div className="form-field switch-field">
                      <span className="form-label">Status do produto</span>
                      <label className="toggle-switch">
                        <input
                          type="checkbox"
                          checked={form.ativo}
                          onChange={(event) => setForm({ ...form, ativo: event.target.checked })}
                        />
                        <span className="slider" />
                      </label>
                      <span className={`toggle-status ${form.ativo ? "active" : "inactive"}`}>
                        {form.ativo ? "Produto ativo" : "Produto inativo"}
                      </span>
                    </div>
                  </div>
                </section>

                <section className="form-section">
                  <div className="section-header">
                    <p className="form-section-title">Módulos e adicionais</p>
                    <button type="button" className="ghost" onClick={addModulo}>
                      + Adicionar módulo
                    </button>
                  </div>
                  <div className="modules-grid">
                    {form.modulos.map((modulo, index) => (
                      <article key={index} className="module-card">
                        <header className="module-card-header">
                          <p>Módulo #{index + 1}</p>
                          {form.modulos.length > 1 && (
                            <button
                              type="button"
                              className="ghost small"
                              onClick={() => removeModulo(index)}
                            >
                              Remover
                            </button>
                          )}
                        </header>
                        <label className="form-field">
                          <span className="form-label">Nome</span>
                          <input
                            value={modulo.nome}
                            onChange={(event) => handleModuloChange(index, "nome", event.target.value)}
                            placeholder="Nome do módulo"
                          />
                        </label>
                        <label className="form-field">
                          <span className="form-label">Descrição</span>
                          <textarea
                            className="textarea"
                            rows={2}
                            value={modulo.descricao}
                            onChange={(event) => handleModuloChange(index, "descricao", event.target.value)}
                          />
                        </label>
                        <div className="form-row">
                          <label className="form-field">
                            <span className="form-label">Custo adicional</span>
                            <input
                              type="number"
                              min="0"
                              step="0.01"
                              value={modulo.custoAdicional}
                              onChange={(event) =>
                                handleModuloChange(index, "custoAdicional", event.target.value)
                              }
                            />
                          </label>
                          <label className="form-field">
                            <span className="form-label">Valor adicional</span>
                            <input
                              type="number"
                              min="0"
                              step="0.01"
                              value={modulo.valorAdicional}
                              onChange={(event) =>
                                handleModuloChange(index, "valorAdicional", event.target.value)
                              }
                            />
                          </label>
                        </div>
                        <div className="form-field switch-field">
                          <span className="form-label">Status do módulo</span>
                          <label className="toggle-switch">
                            <input
                              type="checkbox"
                              checked={modulo.ativo}
                              onChange={(event) => handleModuloChange(index, "ativo", event.target.checked)}
                            />
                            <span className="slider" />
                          </label>
                          <span className={`toggle-status ${modulo.ativo ? "active" : "inactive"}`}>
                            {modulo.ativo ? "Módulo ativo" : "Módulo inativo"}
                          </span>
                        </div>
                      </article>
                    ))}
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

      {modulesViewer && (
        <div className="modal-overlay" onClick={() => setModulesViewer(null)}>
          <div className="modal" onClick={(event) => event.stopPropagation()}>
            <header className="modal-header">
              <h3>Módulos de {modulesViewer.nome}</h3>
              <button className="ghost" onClick={() => setModulesViewer(null)}>
                Fechar
              </button>
            </header>
            <div className="modal-content">
              {modulesViewer.modulos && modulesViewer.modulos.length > 0 ? (
                <ul className="modules-view-list">
                  {modulesViewer.modulos.map((modulo) => (
                    <li key={modulo.id}>
                      <div>
                        <strong>{modulo.nome}</strong>
                        {modulo.descricao && <small>{modulo.descricao}</small>}
                      </div>
                      <div className="modules-view-values">
                        <span>Custo: {currencyFormatter.format(modulo.custoAdicional)}</span>
                        <span>Valor: {currencyFormatter.format(modulo.valorAdicional)}</span>
                      </div>
                    </li>
                  ))}
                </ul>
              ) : (
                <p className="muted">Nenhum módulo cadastrado para este produto.</p>
              )}
            </div>
          </div>
        </div>
      )}
    </AppLayout>
  );
};

export default ProdutosPage;

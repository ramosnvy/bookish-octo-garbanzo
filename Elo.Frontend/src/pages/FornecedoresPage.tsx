import { useEffect, useMemo, useState } from "react";
import AppLayout from "../components/AppLayout";
import {
  CreateFornecedorRequest,
  FornecedorCategoriaDto,
  FornecedorDto,
  FornecedorEnderecoInput,
} from "../models";
import { FornecedorService } from "../services/FornecedorService";
import { FornecedorCategoriaService } from "../services/FornecedorCategoriaService";
import { useAuth } from "../context/AuthContext";

const statusOptions = [
  { value: 1, label: "Ativo", badge: "ativo" },
  { value: 2, label: "Inativo", badge: "inativo" },
  { value: 3, label: "Suspenso", badge: "suspenso" },
  { value: 4, label: "Cancelado", badge: "cancelado" },
];

const statusMap = statusOptions.reduce<Record<number, { label: string; badge: string }>>((acc, option) => {
  acc[option.value] = { label: option.label, badge: option.badge };
  return acc;
}, {});

const statusNameToValue: Record<string, number> = {
  ativo: 1,
  inativo: 2,
  suspenso: 3,
  cancelado: 4,
};

const normalizeStatusValue = (value: number | string): number => {
  if (typeof value === "number") {
    return value;
  }

  const parsed = Number(value);
  if (!Number.isNaN(parsed)) {
    return parsed;
  }

  const normalized = value.toLowerCase();
  return statusNameToValue[normalized] ?? 0;
};

const novoEndereco = (): FornecedorEnderecoInput => ({
  logradouro: "",
  numero: "",
  bairro: "",
  cidade: "",
  estado: "",
  cep: "",
  complemento: "",
});

const FornecedoresPage = () => {
  const { selectedEmpresaId, user } = useAuth();
  const [fornecedores, setFornecedores] = useState<FornecedorDto[]>([]);
  const [categorias, setCategorias] = useState<FornecedorCategoriaDto[]>([]);
  const [form, setForm] = useState<CreateFornecedorRequest>({
    nome: "",
    cnpj: "",
    email: "",
    telefone: "",
    categoriaId: 0,
    status: 1,
    enderecos: [novoEndereco()],
  });
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState<number | "all">("all");
  const [categoriaFilter, setCategoriaFilter] = useState<number | "all">("all");
  const [modalOpen, setModalOpen] = useState(false);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [selectedFornecedor, setSelectedFornecedor] = useState<FornecedorDto | null>(null);
  const normalizedRole = user?.role?.toLowerCase();
  const isGlobalAdmin = normalizedRole === "admin" && !user?.empresaId;

  useEffect(() => {
    FornecedorService.getAll(selectedEmpresaId).then(setFornecedores);
  }, [selectedEmpresaId]);

  useEffect(() => {
    FornecedorCategoriaService.getAll(selectedEmpresaId).then(setCategorias);
  }, [selectedEmpresaId]);

  useEffect(() => {
    if (!editingId && categorias.length > 0 && !form.categoriaId) {
      setForm((prev) => ({ ...prev, categoriaId: categorias[0].id }));
    }
  }, [categorias, editingId, form.categoriaId]);

  const filteredFornecedores = useMemo(() => {
    let result = fornecedores;
    if (search) {
      const normalized = search.toLowerCase();
      result = result.filter((f) =>
        [f.nome, f.email, f.cnpj, f.categoriaNome || ""].some((field) => field.toLowerCase().includes(normalized))
      );
    }
    if (statusFilter !== "all") {
      result = result.filter((f) => normalizeStatusValue(f.status) === statusFilter);
    }
    if (categoriaFilter !== "all") {
      result = result.filter((f) => f.categoriaId === categoriaFilter);
    }
    return result;
  }, [fornecedores, search, statusFilter, categoriaFilter]);

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    if (isGlobalAdmin) {
      return;
    }
    if (!form.categoriaId) {
      alert("Selecione uma categoria antes de salvar.");
      return;
    }
    const enderecosValidos = form.enderecos.filter((endereco) => endereco.logradouro.trim().length > 0);
    const payload: CreateFornecedorRequest = {
      ...form,
      enderecos: enderecosValidos.length ? enderecosValidos : [novoEndereco()],
    };

    if (editingId) {
      const atualizado = await FornecedorService.update(editingId, payload);
      setFornecedores((prev) => prev.map((f) => (f.id === atualizado.id ? atualizado : f)));
    } else {
      const novo = await FornecedorService.create(payload);
      setFornecedores((prev) => [...prev, novo]);
    }

    setForm({
      nome: "",
      cnpj: "",
      email: "",
      telefone: "",
      categoriaId: categorias[0]?.id ?? 0,
      status: 1,
      enderecos: [novoEndereco()],
    });
    setEditingId(null);
    setModalOpen(false);
  };

  const handleOpenCreate = () => {
    if (isGlobalAdmin) {
      return;
    }
    setEditingId(null);
    setForm({
      nome: "",
      cnpj: "",
      email: "",
      telefone: "",
      categoriaId: categorias[0]?.id ?? 0,
      status: 1,
      enderecos: [novoEndereco()],
    });
    setModalOpen(true);
  };

  const handleOpenEdit = (fornecedor: FornecedorDto) => {
    if (isGlobalAdmin) {
      return;
    }
    setEditingId(fornecedor.id);
    setForm({
      nome: fornecedor.nome,
      cnpj: fornecedor.cnpj,
      email: fornecedor.email,
      telefone: fornecedor.telefone,
      categoriaId: fornecedor.categoriaId ?? categorias[0]?.id ?? 0,
      status: normalizeStatusValue(fornecedor.status),
      enderecos: fornecedor.enderecos.length
        ? fornecedor.enderecos.map((endereco) => ({
            logradouro: endereco.logradouro,
            numero: endereco.numero,
            bairro: endereco.bairro,
            cidade: endereco.cidade,
            estado: endereco.estado,
            cep: endereco.cep,
            complemento: endereco.complemento,
          }))
        : [novoEndereco()],
    });
    setModalOpen(true);
  };

  const handleAddressChange = (index: number, field: keyof FornecedorEnderecoInput, value: string) => {
    setForm((prev) => {
      const next = prev.enderecos.map((endereco, idx) => (idx === index ? { ...endereco, [field]: value } : endereco));
      return { ...prev, enderecos: next };
    });
  };

  const addAddress = () => {
    setForm((prev) => ({ ...prev, enderecos: [...prev.enderecos, novoEndereco()] }));
  };

  const removeAddress = (index: number) => {
    setForm((prev) => {
      if (prev.enderecos.length === 1) return prev;
      return { ...prev, enderecos: prev.enderecos.filter((_, idx) => idx !== index) };
    });
  };

  return (
    <AppLayout title="Fornecedores" subtitle="Cadastre e acompanhe parceiros de suprimentos">
      {isGlobalAdmin && (
        <div className="toast info">Administradores globais possuem acesso somente leitura nesta tela.</div>
      )}
      <section className="filters-bar">
        <input
          className="search-input"
          placeholder="Pesquisar fornecedores..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <div className="filters-group">
          <div className="select-wrapper">
            <select
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value === "all" ? "all" : Number(e.target.value))}
            >
              <option value="all">Todos os status</option>
              {statusOptions.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </div>
          <div className="select-wrapper">
            <select
              value={categoriaFilter === "all" ? "all" : categoriaFilter.toString()}
              onChange={(e) => setCategoriaFilter(e.target.value === "all" ? "all" : Number(e.target.value))}
              disabled={!categorias.length}
            >
              <option value="all">Todas categorias</option>
              {categorias.map((categoria) => (
                <option key={categoria.id} value={categoria.id}>
                  {categoria.nome}
                </option>
              ))}
            </select>
          </div>
        </div>
        {!isGlobalAdmin && (
          <div className="filters-actions">
            <button className="primary" onClick={handleOpenCreate}>
              + Adicionar fornecedor
            </button>
          </div>
        )}
      </section>

      <section className="card table-wrapper">
        <table>
          <thead>
            <tr>
              <th>Fornecedor</th>
              <th>Contato</th>
              <th>Categoria</th>
              <th>Status</th>
              <th>Ações</th>
            </tr>
          </thead>
          <tbody>
            {filteredFornecedores.length === 0 ? (
              <tr>
                <td colSpan={5}>Nenhum fornecedor encontrado.</td>
              </tr>
            ) : (
              filteredFornecedores.map((fornecedor) => {
                const fornecedorStatus = normalizeStatusValue(fornecedor.status);
                const info = statusMap[fornecedorStatus] ?? { label: "Desconhecido", badge: "inativo" };
                return (
                  <tr key={fornecedor.id}>
                  <td>
                      <p className="table-title">{fornecedor.nome}</p>
                      <small>{fornecedor.cnpj}</small>
                    </td>
                    <td>
                      <p>{fornecedor.email}</p>
                      <small>{fornecedor.telefone || "-"}</small>
                    </td>
                    <td>{fornecedor.categoriaNome || "-"}</td>
                    <td>
                      <span className={`badge ${info.badge}`}>{info.label}</span>
                    </td>
                    <td>
                      <div className="table-actions">
                        {!isGlobalAdmin && (
                          <button className="ghost" onClick={() => handleOpenEdit(fornecedor)}>
                            Editar
                          </button>
                        )}
                        <button className="ghost" onClick={() => setSelectedFornecedor(fornecedor)}>
                          Endereços
                        </button>
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
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <header className="modal-header">
              <h3>{editingId ? "Editar fornecedor" : "Novo fornecedor"}</h3>
              <button className="ghost" onClick={() => setModalOpen(false)}>
                Fechar
              </button>
            </header>
            <div className="modal-content">
              <form className="form-inline" onSubmit={handleSubmit}>
                <input placeholder="Nome" value={form.nome} onChange={(e) => setForm({ ...form, nome: e.target.value })} required />
                <input placeholder="CNPJ" value={form.cnpj} onChange={(e) => setForm({ ...form, cnpj: e.target.value })} required />
                <input placeholder="Email" type="email" value={form.email} onChange={(e) => setForm({ ...form, email: e.target.value })} required />
                <input placeholder="Telefone" value={form.telefone} onChange={(e) => setForm({ ...form, telefone: e.target.value })} />
                <div className="select-wrapper full">
                  <select
                    value={form.categoriaId || ""}
                    onChange={(e) => setForm({ ...form, categoriaId: Number(e.target.value) })}
                    required
                    disabled={!categorias.length}
                  >
                    <option value="">
                      {categorias.length ? "Selecione uma categoria" : "Cadastre uma categoria primeiro"}
                    </option>
                    {categorias.map((categoria) => (
                      <option key={categoria.id} value={categoria.id}>
                        {categoria.nome}
                      </option>
                    ))}
                  </select>
                </div>
                <select value={form.status} onChange={(e) => setForm({ ...form, status: Number(e.target.value) })}>
                  {statusOptions.map((option) => (
                    <option key={option.value} value={option.value}>
                      {option.label}
                    </option>
                  ))}
                </select>
                <div className="addresses-group">
                  <div className="addresses-header">
                    <h4>Endereços</h4>
                    <button type="button" className="ghost" onClick={addAddress}>
                      + Adicionar endereço
                    </button>
                  </div>
                  {form.enderecos.map((endereco, index) => (
                    <div key={index} className="address-card">
                      <input
                        placeholder="Logradouro"
                        value={endereco.logradouro}
                        onChange={(e) => handleAddressChange(index, "logradouro", e.target.value)}
                        required={index === 0}
                      />
                      <div className="address-inline">
                        <input
                          placeholder="Número"
                          value={endereco.numero}
                          onChange={(e) => handleAddressChange(index, "numero", e.target.value)}
                        />
                        <input
                          placeholder="Bairro"
                          value={endereco.bairro}
                          onChange={(e) => handleAddressChange(index, "bairro", e.target.value)}
                        />
                      </div>
                      <div className="address-inline">
                        <input
                          placeholder="Cidade"
                          value={endereco.cidade}
                          onChange={(e) => handleAddressChange(index, "cidade", e.target.value)}
                        />
                        <input
                          placeholder="Estado"
                          value={endereco.estado}
                          onChange={(e) => handleAddressChange(index, "estado", e.target.value)}
                        />
                      </div>
                      <div className="address-inline">
                        <input
                          placeholder="CEP"
                          value={endereco.cep}
                          onChange={(e) => handleAddressChange(index, "cep", e.target.value)}
                        />
                        <input
                          placeholder="Complemento"
                          value={endereco.complemento}
                          onChange={(e) => handleAddressChange(index, "complemento", e.target.value)}
                        />
                      </div>
                      {form.enderecos.length > 1 && (
                        <button type="button" className="ghost full" onClick={() => removeAddress(index)}>
                          Remover
                        </button>
                      )}
                    </div>
                  ))}
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
      {selectedFornecedor && (
        <div className="modal-overlay" onClick={() => setSelectedFornecedor(null)}>
          <div className="modal small" onClick={(e) => e.stopPropagation()}>
            <header className="modal-header">
              <h3>Endereços de {selectedFornecedor.nome}</h3>
              <button className="ghost" onClick={() => setSelectedFornecedor(null)}>
                Fechar
              </button>
            </header>
            <div className="modal-content addresses-view">
              {selectedFornecedor.enderecos.length === 0 ? (
                <p className="muted">Nenhum endereço cadastrado para este fornecedor.</p>
              ) : (
                <ul className="address-list">
                  {selectedFornecedor.enderecos.map((endereco, index) => (
                    <li key={`${endereco.id}-${index}`}>
                      <strong>{endereco.logradouro}</strong>, {endereco.numero} - {endereco.bairro}
                      <br />
                      {endereco.cidade}/{endereco.estado} · {endereco.cep}
                      {endereco.complemento && <span> · {endereco.complemento}</span>}
                    </li>
                  ))}
                </ul>
              )}
            </div>
          </div>
        </div>
      )}
    </AppLayout>
  );
};

export default FornecedoresPage;

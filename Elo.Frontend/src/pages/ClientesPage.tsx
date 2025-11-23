import { useEffect, useMemo, useState } from "react";
import AppLayout from "../components/AppLayout";
import { ClienteDto, CreateClienteRequest, ClienteEnderecoInput } from "../models";
import { ClienteService } from "../services/ClienteService";
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

const novoEndereco = (): ClienteEnderecoInput => ({
  logradouro: "",
  numero: "",
  bairro: "",
  cidade: "",
  estado: "",
  cep: "",
  complemento: "",
});

const emptyForm: CreateClienteRequest = {
  nome: "",
  cnpjCpf: "",
  email: "",
  telefone: "",
  status: 1,
  enderecos: [novoEndereco()],
};

const ClientesPage = () => {
  const { selectedEmpresaId, user } = useAuth();
  const [clientes, setClientes] = useState<ClienteDto[]>([]);
  const [form, setForm] = useState<CreateClienteRequest>(emptyForm);
  const [search, setSearch] = useState("");
  const [statusFilter, setStatusFilter] = useState<number | "all">("all");
  const [modalOpen, setModalOpen] = useState(false);
  const [editingClienteId, setEditingClienteId] = useState<number | null>(null);
  const [selectedCliente, setSelectedCliente] = useState<ClienteDto | null>(null);
  const normalizedRole = user?.role?.toLowerCase();
  const isGlobalAdmin = normalizedRole === "admin" && !user?.empresaId;

  useEffect(() => {
    ClienteService.getAll(selectedEmpresaId).then(setClientes);
  }, [selectedEmpresaId]);

  const filteredClientes = useMemo(() => {
    let result = clientes;
    if (search) {
      const normalized = search.toLowerCase();
      result = result.filter((c) =>
        [c.nome, c.email, c.cnpjCpf].some((field) => field.toLowerCase().includes(normalized))
      );
    }
    if (statusFilter !== "all") {
      result = result.filter((c) => normalizeStatusValue(c.status) === statusFilter);
    }
    return result;
  }, [clientes, search, statusFilter]);

  const handleSubmit = async (event: React.FormEvent) => {
    event.preventDefault();
    if (isGlobalAdmin) {
      return;
    }
    const enderecosValidos = form.enderecos.filter((endereco) => endereco.logradouro.trim().length > 0);
    const payload: CreateClienteRequest = {
      ...form,
      enderecos: enderecosValidos.length ? enderecosValidos : [novoEndereco()],
    };

    if (editingClienteId) {
      const atualizado = await ClienteService.update(editingClienteId, payload);
      setClientes((prev) => prev.map((cliente) => (cliente.id === atualizado.id ? atualizado : cliente)));
    } else {
      const novoCliente = await ClienteService.create(payload);
      setClientes((prev) => [...prev, novoCliente]);
    }

    setForm(emptyForm);
    setEditingClienteId(null);
    setModalOpen(false);
  };

  const handleOpenCreate = () => {
    if (isGlobalAdmin) {
      return;
    }
    setForm(emptyForm);
    setEditingClienteId(null);
    setModalOpen(true);
  };

  const handleOpenEdit = (cliente: ClienteDto) => {
    if (isGlobalAdmin) {
      return;
    }
    setEditingClienteId(cliente.id);
    setForm({
      nome: cliente.nome,
      cnpjCpf: cliente.cnpjCpf,
      email: cliente.email,
      telefone: cliente.telefone,
      status: normalizeStatusValue(cliente.status),
      enderecos: cliente.enderecos.length
        ? cliente.enderecos.map((endereco) => ({
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

  const handleAddressChange = (index: number, field: keyof ClienteEnderecoInput, value: string) => {
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

  const stats = [
    { label: "Total", value: clientes.length },
    { label: "Ativos", value: clientes.filter((c) => normalizeStatusValue(c.status) === 1).length },
    { label: "Suspensos", value: clientes.filter((c) => normalizeStatusValue(c.status) === 3).length },
    { label: "Cancelados", value: clientes.filter((c) => normalizeStatusValue(c.status) === 4).length },
  ];

  return (
    <AppLayout title="Clientes" subtitle="Gerencie seus relacionamentos e oportunidades de negócio">
      {isGlobalAdmin && (
        <div className="toast info">Administradores globais possuem acesso somente leitura nesta tela.</div>
      )}
      <section className="filters-bar">
        <input
          className="search-input"
          placeholder="Pesquisar clientes..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <select className="ghost" value={statusFilter} onChange={(e) => setStatusFilter(e.target.value === "all" ? "all" : Number(e.target.value))}>
          <option value="all">Todos</option>
          {statusOptions.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>
        {!isGlobalAdmin && (
          <button className="primary" onClick={handleOpenCreate}>
            + Adicionar cliente
          </button>
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
              <th>Cliente</th>
              <th>Contato</th>
              <th>Status</th>
              <th>Ações</th>
            </tr>
          </thead>
          <tbody>
            {filteredClientes.length === 0 ? (
              <tr>
                <td colSpan={4}>Nenhum cliente encontrado.</td>
              </tr>
            ) : (
              filteredClientes.map((cliente) => {
                const clienteStatusValue = normalizeStatusValue(cliente.status);
                const info = statusMap[clienteStatusValue] ?? { label: "Desconhecido", badge: "inativo" };
                return (
                  <tr key={cliente.id}>
                    <td>
                      <p className="table-title">{cliente.nome}</p>
                      <small>{cliente.cnpjCpf}</small>
                    </td>
                    <td>
                      <p>{cliente.email}</p>
                      <small>{cliente.telefone || "-"}</small>
                    </td>
                    <td>
                      <span className={`badge ${info.badge}`}>{info.label}</span>
                    </td>
                    <td>
                      <div className="table-actions">
                        {!isGlobalAdmin && (
                        <button className="ghost" onClick={() => handleOpenEdit(cliente)}>
                          Editar
                        </button>
                        )}
                        {cliente.enderecos?.length ? (
                          <button className="ghost" onClick={() => setSelectedCliente(cliente)}>
                            Ver endereços
                          </button>
                        ) : null}
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
              <h3>{editingClienteId ? "Editar cliente" : "Novo cliente"}</h3>
              <button className="ghost" onClick={() => setModalOpen(false)}>
                Fechar
              </button>
            </header>
            <div className="modal-content">
              <form className="form-inline" onSubmit={handleSubmit}>
                <input placeholder="Nome" value={form.nome} onChange={(e) => setForm({ ...form, nome: e.target.value })} required />
              <input
                placeholder="CNPJ/CPF"
                value={form.cnpjCpf}
                onChange={(e) => setForm({ ...form, cnpjCpf: e.target.value })}
                required
              />
              <input
                placeholder="Email"
                type="email"
                value={form.email}
                onChange={(e) => setForm({ ...form, email: e.target.value })}
                required
              />
              <input placeholder="Telefone" value={form.telefone} onChange={(e) => setForm({ ...form, telefone: e.target.value })} />
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
                    {editingClienteId ? "Atualizar" : "Salvar"}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}

      {selectedCliente && (
        <div className="modal-overlay" onClick={() => setSelectedCliente(null)}>
          <div className="modal" onClick={(e) => e.stopPropagation()}>
            <header className="modal-header">
              <h3>Endereços de {selectedCliente.nome}</h3>
              <button className="ghost" onClick={() => setSelectedCliente(null)}>
                Fechar
              </button>
            </header>
            <div className="modal-content">
              {selectedCliente.enderecos.length === 0 ? (
                <p>Cliente sem endereços cadastrados.</p>
              ) : (
                <div className="addresses-group">
                  {selectedCliente.enderecos.map((endereco, index) => (
                    <div key={endereco.id ?? index} className="address-card">
                      <strong>{endereco.logradouro}</strong>
                      <div className="address-inline">
                        <span>Número: {endereco.numero || "-"}</span>
                        <span>Bairro: {endereco.bairro || "-"}</span>
                      </div>
                      <div className="address-inline">
                        <span>Cidade: {endereco.cidade || "-"}</span>
                        <span>Estado: {endereco.estado || "-"}</span>
                      </div>
                      <div className="address-inline">
                        <span>CEP: {endereco.cep || "-"}</span>
                        <span>Complemento: {endereco.complemento || "-"}</span>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>
        </div>
      )}
    </AppLayout>
  );
};

export default ClientesPage;

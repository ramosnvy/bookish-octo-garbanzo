export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  nome: string;
  email: string;
  role: string;
  empresaId?: number | null;
}

export interface ClienteDto {
  id: number;
  nome: string;
  cnpjCpf: string;
  email: string;
  telefone: string;
  status: number | string;
  enderecos: ClienteEndereco[];
}

export interface CreateClienteRequest {
  nome: string;
  cnpjCpf: string;
  email: string;
  telefone: string;
  status: number;
  enderecos: ClienteEnderecoInput[];
}

export interface ClienteEndereco {
  id: number;
  logradouro: string;
  numero: string;
  bairro: string;
  cidade: string;
  estado: string;
  cep: string;
  complemento: string;
}

export interface ClienteEnderecoInput {
  logradouro: string;
  numero: string;
  bairro: string;
  cidade: string;
  estado: string;
  cep: string;
  complemento: string;
}

export type FornecedorEndereco = ClienteEndereco;
export type FornecedorEnderecoInput = ClienteEnderecoInput;

export interface FornecedorDto {
  id: number;
  nome: string;
  cnpj: string;
  email: string;
  telefone: string;
  categoriaId?: number;
  categoriaNome: string;
  status: number | string;
  enderecos: FornecedorEndereco[];
}

export interface CreateFornecedorRequest {
  nome: string;
  cnpj: string;
  email: string;
  telefone: string;
  categoriaId: number;
  status: number;
  enderecos: FornecedorEnderecoInput[];
}

export interface FornecedorCategoriaDto {
  id: number;
  nome: string;
  ativo: boolean;
}

export interface ProdutoDto {
  id: number;
  nome: string;
  descricao: string;
  valorCusto: number;
  valorRevenda: number;
  margemLucro: number;
  ativo: boolean;
  createdAt: string;
  updatedAt?: string;
  fornecedorId?: number | null;
  fornecedorNome?: string | null;
  valorTotalComAdicionais?: number;
  modulos?: ProdutoModuloDto[];
}

export interface CreateProdutoRequest {
  nome: string;
  descricao: string;
  valorCusto: number;
  valorRevenda: number;
  ativo: boolean;
  fornecedorId?: number | null;
  modulos?: ProdutoModuloInput[];
}

export interface ProdutoModuloDto {
  id: number;
  nome: string;
  descricao: string;
  valorAdicional: number;
  custoAdicional: number;
  ativo: boolean;
}

export interface ProdutoModuloInput {
  nome: string;
  descricao: string;
  valorAdicional: number;
  custoAdicional: number;
  ativo: boolean;
}

export interface UserDto {
  id: number;
  nome: string;
  email: string;
  role: string;
  empresaId?: number | null;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateUserRequest {
  nome: string;
  email: string;
  password: string;
  role: string;
  empresaId?: number | null;
}

export interface UpdateUserRequest {
  nome: string;
  email: string;
  role: string;
  empresaId?: number | null;
}

export interface EmpresaDto {
  id: number;
  nome: string;
  documento: string;
  emailContato: string;
  telefoneContato: string;
  ativo: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface CreateEmpresaRequest {
  nome: string;
  documento: string;
  emailContato: string;
  telefoneContato: string;
  ativo: boolean;
  usuarioInicial: {
    nome: string;
    email: string;
    password: string;
  };
}

export interface UpdateEmpresaRequest {
  nome: string;
  documento: string;
  emailContato: string;
  telefoneContato: string;
  ativo: boolean;
}

export enum ContaStatus {
  Pendente = 1,
  Pago = 2,
  Vencido = 3,
  Cancelado = 4,
}

export enum FormaPagamento {
  Dinheiro = 1,
  Pix = 2,
  CartaoCredito = 3,
  CartaoDebito = 4,
  Transferencia = 5,
  Boleto = 6,
  Cheque = 7,
}

export interface ContaPagarDto {
  id: number;
  empresaId: number;
  fornecedorId: number;
  fornecedorNome: string;
  descricao: string;
  valor: number;
  dataVencimento: string;
  dataPagamento?: string | null;
  status: ContaStatus;
  categoria: string;
  isRecorrente: boolean;
  totalParcelas: number;
  intervaloDias: number;
  createdAt: string;
  updatedAt?: string;
  itens: ContaPagarItemDto[];
  parcelas: ContaParcelaDto[];
}

export interface ContaReceberDto {
  id: number;
  empresaId: number;
  clienteId: number;
  clienteNome: string;
  descricao: string;
  valor: number;
  dataVencimento: string;
  dataRecebimento?: string | null;
  status: ContaStatus;
  formaPagamento: FormaPagamento;
  isRecorrente: boolean;
  totalParcelas: number;
  intervaloDias: number;
  createdAt: string;
  updatedAt?: string;
  itens: ContaReceberItemDto[];
  parcelas: ContaParcelaDto[];
}

export interface CreateContaPagarRequest {
  fornecedorId: number;
  descricao: string;
  valor: number;
  dataVencimento: string;
  dataPagamento?: string | null;
  status: ContaStatus;
  categoria: string;
  isRecorrente?: boolean;
  numeroParcelas?: number;
  intervaloDias?: number;
  itens?: ContaFinanceiroItemInput[];
}

export interface UpdateContaPagarRequest extends CreateContaPagarRequest {
  id: number;
}

export interface CreateContaReceberRequest {
  clienteId: number;
  descricao: string;
  valor: number;
  dataVencimento: string;
  dataRecebimento?: string | null;
  status: ContaStatus;
  formaPagamento: FormaPagamento;
  isRecorrente?: boolean;
  numeroParcelas?: number;
  intervaloDias?: number;
  itens?: ContaFinanceiroItemInput[];
}

export interface UpdateContaReceberRequest extends CreateContaReceberRequest {
  id: number;
}

export interface ContaFinanceiroItemInput {
  produtoId?: number;
  produtoModuloId?: number;
  produtoModuloIds?: number[];
  descricao: string;
  valor: number;
}

export interface ContaPagarItemDto extends ContaFinanceiroItemInput {
  id: number;
  contaPagarId: number;
}

export interface ContaReceberItemDto extends ContaFinanceiroItemInput {
  id: number;
  contaReceberId: number;
}

export interface ContaParcelaDto {
  id: number;
  numero: number;
  valor: number;
  dataVencimento: string;
  dataPagamento?: string | null;
  status: ContaStatus;
}

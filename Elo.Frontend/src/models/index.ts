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

export type FornecedorPagamentoTipo = "PRE" | "POS";

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
  tipoPagamento: FornecedorPagamentoTipo;
  prazoPagamentoDias: number;
}

export interface CreateFornecedorRequest {
  nome: string;
  cnpj: string;
  email: string;
  telefone: string;
  categoriaId: number;
  status: number;
  enderecos: FornecedorEnderecoInput[];
  tipoPagamento: FornecedorPagamentoTipo;
  prazoPagamentoDias: number;
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

export interface UpdateContaReceberParcelaStatusRequest {
  status: ContaStatus;
  dataRecebimento?: string | null;
}

export interface ContaParcelaDto {
  id: number;
  numero: number;
  valor: number;
  dataVencimento: string;
  dataPagamento?: string | null;
  status: ContaStatus;
}

export interface HistoriaMovimentacaoDto {
  id: number;
  historiaId: number;
  statusAnteriorId: number;
  statusAnteriorNome: string;
  statusNovoId: number;
  statusNovoNome: string;
  usuarioId: number;
  usuarioNome: string;
  dataMovimentacao: string;
  observacoes?: string | null;
}

export interface HistoriaDto {
  id: number;
  clienteId: number;
  clienteNome: string;
  produtoId: number;
  produtoNome: string;
  statusId: number;
  statusNome: string;
  statusCor?: string | null;
  statusFechaHistoria: boolean;
  tipoId: number;
  tipoNome: string;
  tipoDescricao?: string | null;
  usuarioResponsavelId?: number | null;
  usuarioResponsavelNome?: string | null;
  previsaoDias?: number | null;
  dataInicio: string;
  dataFinalizacao?: string | null;
  observacoes?: string | null;
  createdAt: string;
  updatedAt?: string | null;
  movimentacoes: HistoriaMovimentacaoDto[];
  produtos: HistoriaProdutoDto[];
}

export interface CreateHistoriaRequest {
  clienteId: number;
  statusId: number;
  tipoId: number;
  usuarioResponsavelId?: number | null;
  dataInicio?: string | null;
  dataFinalizacao?: string | null;
  observacoes?: string | null;
  produtos: HistoriaProdutoInputDto[];
}

export interface UpdateHistoriaRequest extends CreateHistoriaRequest {
  id: number;
}

export interface HistoriaProdutoDto {
  produtoId: number;
  produtoNome: string;
  produtoModuloIds: number[];
  produtoModuloNomes: string[];
}

export interface HistoriaProdutoInputDto {
  produtoId: number;
  produtoModuloIds: number[];
}

export interface CreateHistoriaMovimentacaoRequest {
  statusNovoId: number;
  observacoes?: string | null;
}

export interface HistoriaStatusConfigDto {
  id: number;
  nome: string;
  descricao?: string | null;
  cor?: string | null;
  fechaHistoria: boolean;
  ordem: number;
  ativo: boolean;
  createdAt: string;
  updatedAt?: string | null;
}

export interface CreateHistoriaStatusConfigRequest {
  nome: string;
  descricao?: string | null;
  cor?: string | null;
  fechaHistoria: boolean;
  ordem: number;
  ativo: boolean;
}

export interface UpdateHistoriaStatusConfigRequest extends CreateHistoriaStatusConfigRequest {
  id: number;
}

export interface HistoriaTipoConfigDto {
  id: number;
  nome: string;
  descricao?: string | null;
  ordem: number;
  ativo: boolean;
  createdAt: string;
  updatedAt?: string | null;
}

export interface CreateHistoriaTipoConfigRequest {
  nome: string;
  descricao?: string | null;
  ordem: number;
  ativo: boolean;
}

export interface UpdateHistoriaTipoConfigRequest extends CreateHistoriaTipoConfigRequest {
  id: number;
}

export enum TicketStatus {
  Aberto = 1,
  EmAndamento = 2,
  PendenteCliente = 3,
  Resolvido = 4,
  Fechado = 5,
  Cancelado = 6,
}

export enum TicketTipo {
  Suporte = 1,
  Bug = 2,
  Melhoria = 3,
  Duvida = 4,
  Incidente = 5,
}

export enum TicketPrioridade {
  Baixa = 1,
  Media = 2,
  Alta = 3,
  Critica = 4,
}

export interface RespostaTicketDto {
  id: number;
  ticketId: number;
  usuarioId: number;
  usuarioNome: string;
  mensagem: string;
  dataResposta: string;
  isInterna: boolean;
}

export interface TicketDto {
  id: number;
  clienteId: number;
  clienteNome: string;
  titulo: string;
  descricao: string;
  tipo: TicketTipo;
  prioridade: TicketPrioridade;
  status: TicketStatus;
  usuarioAtribuidoId?: number | null;
  usuarioAtribuidoNome?: string | null;
  dataAbertura: string;
  dataFechamento?: string | null;
  createdAt: string;
  updatedAt?: string | null;
  numeroExterno: string;
  respostas: RespostaTicketDto[];
}

export interface CreateTicketRequest {
  clienteId: number;
  titulo: string;
  descricao: string;
  tipo: TicketTipo;
  prioridade: TicketPrioridade;
  status: TicketStatus;
  usuarioAtribuidoId?: number | null;
  numeroExterno?: string;
}

export interface UpdateTicketRequest extends CreateTicketRequest {
  id: number;
  dataFechamento?: string | null;
}

export interface CreateRespostaTicketRequest {
  mensagem: string;
  isInterna: boolean;
}

export interface TicketTipoConfigDto {
  id: number;
  nome: string;
  descricao?: string | null;
  ordem: number;
  ativo: boolean;
  createdAt: string;
  updatedAt?: string | null;
}

export interface CreateTicketTipoConfigRequest {
  nome: string;
  descricao?: string | null;
  ordem: number;
  ativo: boolean;
}

export interface UpdateTicketTipoConfigRequest extends CreateTicketTipoConfigRequest {
  id: number;
}

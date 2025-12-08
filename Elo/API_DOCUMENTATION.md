# Elo API – Documentação de Endpoints

## 1. Visão Geral
- **Base URL**: `https://{host}/api`. Todos os controladores seguem o prefixo `api/` definido em atributos de rota.
- **Autenticação**: JWT Bearer. Envie `Authorization: Bearer {token}` para qualquer rota marcada com `[Authorize]`. Apenas `/api/auth/login` e `/api/auth/register` são públicos.
- **Autorização/Roles**: a maior parte dos endpoints aceita usuários autenticados; alguns exigem perfil `Admin` (por exemplo, `/api/empresas`).
- **Contexto de Empresa**: a API é multi-tenant. Usuários comuns têm o `EmpresaId` inferido do token. Usuários globais podem usar a query `?empresaId=123` para atuar em outra empresa. Métodos que chamam `RequireEmpresaAsync` sempre validarão que um `EmpresaId` existe no token ou na query.
- **Formato**: `Content-Type: application/json; charset=utf-8`. Datas seguem ISO 8601 (`2024-01-31T12:00:00Z`).
- **Enums como texto**: devido ao `JsonStringEnumConverter`, enums sempre são serializados/deserializados como strings (`"Status":"Ativo"`).
- **Paginação/Filtros**: rotas de listagem aceitam `page` e `pageSize` (inteiros) ou filtros específicos; a resposta ainda é um array simples, sem envelope de metadados.
- **Erros**: em falhas conhecidas são retornados `400`, `401`, `403` ou `404` com corpo `{ "message": "descrição" }`. `500` vem do middleware global.
- **CORS**: liberado para `http://localhost` e `:5173/7271`, permitindo credenciais.

## 2. Enumerações suportadas
| Enum | Valores |
| --- | --- |
| `UserRole` | `Admin`, `Manager`, `Employee`, `Client` |
| `Status` (Clientes/Fornecedores) | `Ativo`, `Inativo`, `Suspenso`, `Cancelado` |
| `ContaStatus` | `Pendente`, `Pago`, `Vencido`, `Cancelado` |
| `FormaPagamento` | `Dinheiro`, `PIX`, `CartaoCredito`, `CartaoDebito`, `Transferencia`, `Boleto`, `Cheque` |
| `ServicoPagamentoTipo` | `PrePago`, `PosPago` |
| `TicketPrioridade` | `Baixa`, `Media`, `Alta`, `Critica` |
| `TicketStatus` | `Aberto`, `EmAndamento`, `PendenteCliente`, `Resolvido`, `Fechado`, `Cancelado` |

## 3. Recursos e Endpoints

### 3.1 Autenticação (`/api/auth`)
| Método | Rota | Descrição | Auth |
| --- | --- | --- | --- |
| `POST` | `/api/auth/login` | Gera token JWT | Público |
| `POST` | `/api/auth/register` | Cria usuário inicial e retorna token | Público |
| `GET` | `/api/auth/me` | Retorna perfil do usuário autenticado | JWT |
| `POST` | `/api/auth/logout` | Apenas confirma logout | JWT |

#### POST `/api/auth/login`
- **Body**:
  ```json
  {
    "email": "usuario@empresa.com",
    "password": "string"
  }
  ```
- **Resposta 200** (`LoginResponse`):
  ```json
  {
    "token": "jwt",
    "nome": "Nome Usuário",
    "email": "usuario@empresa.com",
    "role": "Admin",
    "empresaId": 42,
    "expiresAt": "2024-05-31T15:45:00Z"
  }
  ```
- **Erros**: `401` (credenciais inválidas) ou `400` (erros inesperados).

#### POST `/api/auth/register`
- **Body**: `Nome`, `Email`, `Password`, `Role` (enum `UserRole`).
- **Resposta 200**: mesmo formato de `LoginResponse`.
- **Uso**: controlado para criação de contas pela administração do sistema.

#### GET `/api/auth/me`
- Sem body; retorna `UserDto` (`Id`, `Nome`, `Email`, `Role`, `EmpresaId`, timestamps).
- `404` caso usuário não exista mais.

#### POST `/api/auth/logout`
- Sem parâmetros; retorna `{ "message": "Logout realizado com sucesso" }`.

### 3.2 Empresas (`/api/empresas`)
| Método | Rota | Descrição | Auth |
| --- | --- | --- | --- |
| `GET` | `/api/empresas` | Lista todas as empresas | `Admin` |
| `POST` | `/api/empresas` | Cria empresa | `Admin` |
| `PUT` | `/api/empresas/{id}` | Atualiza dados cadastrais | `Admin` |

- **Create/Update Body**: `RazaoSocial`, `NomeFantasia`, `Cnpj`, `Ie`, `Email`, `Telefone`, `Endereco`, `Ativo`.
- **Respostas**: `EmpresaDto` com timestamps.

### 3.3 Usuários (`/api/users`)
| Método | Rota | Descrição | Auth |
| --- | --- | --- | --- |
| `GET` | `/api/users` | Lista usuários da empresa ou global | JWT |
| `GET` | `/api/users/{id}` | Retorna usuário | JWT |
| `POST` | `/api/users` | Cria usuário | JWT |
| `PUT` | `/api/users/{id}` | Atualiza usuário | JWT |
| `DELETE` | `/api/users/{id}` | Remove usuário | JWT |
| `PUT` | `/api/users/{id}/change-password` | Troca senha (self-service) | JWT |

- **Listagem**: filtros `page`, `pageSize`, `search`, `role`, `empresaId` (apenas admins globais).
- **Create/Update Body**: `Nome`, `Email`, `Password` (apenas create), `Role`, `EmpresaId` (opcional, ignorado para usuários comuns). Internamente é validado se o usuário atual pode escolher a empresa.
- **Change Password Body**: `{ "currentPassword": "string", "newPassword": "string" }`.
- **Respostas**: `UserDto`. Delete retorna `204`.

### 3.4 Clientes (`/api/clientes`)
| Método | Rota | Descrição |
| --- | --- | --- |
| `GET` | `/api/clientes` | Lista clientes com filtros/paginação |
| `GET` | `/api/clientes/{id}` | Retorna cliente específico |
| `POST` | `/api/clientes` | Cria cliente |
| `PUT` | `/api/clientes/{id}` | Atualiza cliente |
| `DELETE` | `/api/clientes/{id}` | Exclui cliente |

- **Query**: `page`, `pageSize`, `empresaId` para admins.
- **Body (Create/Update)**: `Nome`, `CnpjCpf`, `Email`, `Telefone`, `Status`, `Enderecos` (lista de objetos com `Logradouro`, `Numero`, `Bairro`, `Cidade`, `Estado`, `Cep`, `Complemento`).
- **Resposta**: `ClienteDto` inclui `Status`, datas de cadastro/alteração e endereços com IDs.

### 3.5 Categorias de Fornecedor (`/api/fornecedorcategorias`)
| Método | Rota | Descrição |
| --- | --- | --- |
| `GET` | `/api/fornecedorcategorias` | Lista categorias da empresa |
| `POST` | `/api/fornecedorcategorias` | Cria categoria |
| `PUT` | `/api/fornecedorcategorias/{id}` | Atualiza categoria |
| `DELETE` | `/api/fornecedorcategorias/{id}` | Remove categoria |

- **Body**: `Nome`, `Ativo`.
- **Resposta**: `FornecedorCategoriaDto`.

### 3.6 Fornecedores (`/api/fornecedores`)
| Método | Rota | Descrição |
| --- | --- | --- |
| `GET` | `/api/fornecedores` | Lista fornecedores (suporta `page`, `pageSize`, `search`, `categoria`, `status`, `empresaId`) |
| `GET` | `/api/fornecedores/{id}` | Retorna fornecedor |
| `POST` | `/api/fornecedores` | Cria fornecedor |
| `PUT` | `/api/fornecedores/{id}` | Atualiza fornecedor |
| `DELETE` | `/api/fornecedores/{id}` | Exclui fornecedor |

- **Body**: `Nome`, `Cnpj`, `Email`, `Telefone`, `CategoriaId`, `Status`, `TipoPagamentoServico` (`PrePago`/`PosPago`), `PrazoPagamentoDias`, `Enderecos`.
- **Resposta**: `FornecedorDto` com `ServicoPagamentoTipo`, `Status`, `Enderecos` detalhados.

### 3.7 Produtos (`/api/produtos`)
| Método | Rota | Descrição |
| --- | --- | --- |
| `GET` | `/api/produtos` | Lista produtos (filtros `page`, `pageSize`, `search`, `ativo`, `valorMinimo`, `valorMaximo`, `empresaId`) |
| `GET` | `/api/produtos/{id}` | Retorna produto |
| `POST` | `/api/produtos` | Cria produto |
| `PUT` | `/api/produtos/{id}` | Atualiza produto |
| `DELETE` | `/api/produtos/{id}` | Exclui produto |
| `POST` | `/api/produtos/calcular-margem` | Calcula margem sem persistir |

- **Body (Create/Update)**: `Nome`, `Descricao`, `ValorCusto`, `ValorRevenda`, `Ativo`, `FornecedorId`, `Modulos` (cada módulo possui `Nome`, `Descricao`, `ValorAdicional`, `CustoAdicional`, `Ativo`).
- **Resposta**: `ProdutoDto` com `MargemLucro` calculada e `ValorTotalComAdicionais`.
- **Calcular Margem**:
  - Body `{ "valorCusto": 100, "valorRevenda": 150 }`.
  - Resposta `200`: `{ "margemLucro": 0.5 }` (50%).

### 3.8 Financeiro

#### 3.8.1 Contas a Pagar (`/api/financeiro/contas-pagar`)
| Método | Rota | Descrição |
| --- | --- | --- |
| `GET` | `/api/financeiro/contas-pagar` | Lista contas; filtros `status`, `dataInicial`, `dataFinal`, `empresaId` |
| `GET` | `/api/financeiro/contas-pagar/calendario` | Agrega contas por dia para dashboards |
| `GET` | `/api/financeiro/contas-pagar/{id}` | Retorna conta |
| `POST` | `/api/financeiro/contas-pagar` | Cria conta |
| `PUT` | `/api/financeiro/contas-pagar/{id}` | Atualiza conta |
| `DELETE` | `/api/financeiro/contas-pagar/{id}` | Remove conta |

- **Body (Create/Update)**:
  ```json
  {
    "fornecedorId": 10,
    "descricao": "Licenças",
    "valor": 1500.0,
    "dataVencimento": "2024-06-10T00:00:00Z",
    "dataPagamento": null,
    "status": "Pendente",
    "categoria": "Software",
    "isRecorrente": true,
    "numeroParcelas": 3,
    "intervaloDias": 30,
    "itens": [
      {
        "produtoId": 5,
        "descricao": "Módulo Cloud",
        "valor": 750.0
      }
    ]
  }
  ```
- **Resposta**: `ContaPagarDto` contendo `FornecedorNome`, `Itens` detalhados e `Parcelas` calculadas (com número, valor, `DataVencimento`, `Status`).
- **Calendário**: retorna lista de objetos `{ "data": "2024-06-10", "valorTotal": 3000.0, "contas": [ { "id": 1, "descricao": "...", "status": "Pendente" } ] }`.

#### 3.8.2 Contas a Receber (`/api/financeiro/contas-receber`)
| Método | Rota | Descrição |
| --- | --- | --- |
| `GET` | `/api/financeiro/contas-receber` | Lista contas; filtros `status`, `dataInicial`, `dataFinal`, `empresaId` |
| `GET` | `/api/financeiro/contas-receber/{id}` | Retorna conta |
| `POST` | `/api/financeiro/contas-receber` | Cria conta |
| `PUT` | `/api/financeiro/contas-receber/{id}` | Atualiza conta |
| `DELETE` | `/api/financeiro/contas-receber/{id}` | Remove conta |
| `PUT` | `/api/financeiro/contas-receber/{contaId}/parcelas/{parcelaId}/status` | Atualiza status de parcela |

- **Body (Create/Update)**: mesmo formato de contas a pagar, mas substituindo `clienteId`, `dataRecebimento`, `formaPagamento`, `itens` ligados a produtos vendidos. Campos `isRecorrente`, `numeroParcelas`, `intervaloDias` definem geração de parcelas automáticas.
- **Resposta**: `ContaReceberDto` com `ClienteNome`, `FormaPagamento`, `Itens` e `Parcelas`.
- **Atualizar parcela** Body:
  ```json
  {
    "status": "Pago",
    "dataRecebimento": "2024-06-05T00:00:00Z"
  }
  ```

### 3.9 Histórias (implantações)

#### 3.9.1 Status (`/api/historia-status`)
| Método | Rota | Descrição |
| --- | --- | --- |
| `GET` | `/api/historia-status` | Lista status configurados |
| `POST` | `/api/historia-status` | Cria status |
| `PUT` | `/api/historia-status/{id}` | Atualiza status |
| `DELETE` | `/api/historia-status/{id}` | Exclui status |

- **Body**: `Nome`, `Descricao`, `Cor` (hex opcional), `FechaHistoria` (bool), `Ordem`, `Ativo`.
- **Resposta**: `HistoriaStatusDto`.

#### 3.9.2 Tipos (`/api/historia-tipos`)
| Método | Rota | Descrição |
| --- | --- | --- |
| `GET` | `/api/historia-tipos` | Lista tipos |
| `POST` | `/api/historia-tipos` | Cria tipo |
| `PUT` | `/api/historia-tipos/{id}` | Atualiza tipo |
| `DELETE` | `/api/historia-tipos/{id}` | Remove tipo |

- **Body**: `Nome`, `Descricao`, `Ordem`, `Ativo`.

#### 3.9.3 Histórias (`/api/historias`)
| Método | Rota | Descrição |
| --- | --- | --- |
| `GET` | `/api/historias` | Lista histórias com filtros |
| `GET` | `/api/historias/kanban` | Retorna visão kanban (histórias + status + tipos) |
| `GET` | `/api/historias/{id}` | Retorna uma história |
| `POST` | `/api/historias` | Cria história |
| `PUT` | `/api/historias/{id}` | Atualiza história |
| `DELETE` | `/api/historias/{id}` | Remove história |
| `POST` | `/api/historias/{id}/movimentacoes` | Adiciona movimentação/status |

- **Filtros**: `statusId`, `tipoId`, `clienteId`, `produtoId`, `usuarioResponsavelId`, `dataInicio`, `dataFim`, `empresaId`.
- **Body (Create/Update)**:
  ```json
  {
    "clienteId": 8,
    "statusId": 2,
    "tipoId": 1,
    "usuarioResponsavelId": 15,
    "previsaoDias": 30,
    "observacoes": "Kickoff agendado",
    "produtos": [
      { "produtoId": 5, "produtoModuloIds": [1,2] }
    ]
  }
  ```
- **Resposta**: `HistoriaDto` com `Produtos`, `Movimentacoes` (histórico de transições) e campos como `StatusNome`, `TipoNome`, `UsuarioResponsavelNome`, `DataInicio`, `DataFinalizacao`.
- **Kanban**: retorna objeto `{ "historias": [...], "statuses": [...], "tipos": [...] }` para montar colunas e cartões.
- **Movimentação** Body: `{ "statusNovoId": 3, "observacoes": "Cliente aprovou" }`; resposta é a história atualizada.

### 3.10 Tickets

#### 3.10.1 Tipos (`/api/ticket-tipos`)
| Método | Rota | Descrição |
| --- | --- | --- |
| `GET` | `/api/ticket-tipos` | Lista tipos de ticket |
| `POST` | `/api/ticket-tipos` | Cria tipo |
| `PUT` | `/api/ticket-tipos/{id}` | Atualiza tipo |
| `DELETE` | `/api/ticket-tipos/{id}` | Remove tipo |

- **Body**: `Nome`, `Descricao`, `Ordem`, `Ativo`.
- **Resposta**: `TicketTipoDto`.

#### 3.10.2 Tickets (`/api/tickets`)
| Método | Rota | Descrição |
| --- | --- | --- |
| `GET` | `/api/tickets` | Lista tickets (filtros: `status`, `tipoId`, `prioridade`, `clienteId`, `produtoId`, `fornecedorId`, `usuarioAtribuidoId`, `dataAberturaInicio`, `dataAberturaFim`, `empresaId`) |
| `GET` | `/api/tickets/{id}` | Retorna ticket |
| `POST` | `/api/tickets` | Cria ticket |
| `PUT` | `/api/tickets/{id}` | Atualiza ticket |
| `DELETE` | `/api/tickets/{id}` | Remove ticket |
| `POST` | `/api/tickets/{id}/respostas` | Registra resposta/comentário |
| `POST` | `/api/tickets/{id}/anexos` | Anexa arquivo (multipart) |

- **Body (Create/Update)**:
  ```json
  {
    "clienteId": 3,
    "ticketTipoId": 1,
    "titulo": "Erro de login",
    "descricao": "Mensagem 500",
    "prioridade": "Alta",
    "status": "Aberto",
    "usuarioAtribuidoId": 25,
    "produtoId": 6,
    "fornecedorId": null,
    "numeroExterno": "CS-2024-001"
  }
  ```
- **Resposta**: `TicketDto` com coleções `Respostas` (mensagens com `IsInterna`) e `Anexos` (metadados de arquivos), além de datas (`DataAbertura`, `DataFechamento`).
- **Respostas** (`POST /respostas`): Body `{ "mensagem": "Texto", "isInterna": true }`; retorna ticket atualizado. O usuário é inferido da claim `NameIdentifier`.
- **Anexos** (`POST /anexos`): enviar `multipart/form-data` com campo `file`. O conteúdo é carregado em memória e persistido; resposta é o ticket atualizado.

## 4. Comentários para o desenvolvimento do front-end
1. **Gestão de sessão**: guarde `token` e `expiresAt` retornados no login; renove o token antes da expiração (ex.: reautenticar ou implementar fluxo de refresh caso exposto futuramente). Utilize `GET /api/auth/me` para validar sessão ao carregar o app.
2. **Contexto de empresa**: mantenha o `empresaId` escolhido na UI (por exemplo, no perfil do usuário global) e inclua `?empresaId=` nos endpoints que aceitam o parâmetro. Usuários não-admin devem esconder a seleção, pois o back-end ignora valores divergentes.
3. **Tratamento de erros**: todas as falhas relevantes retornam `{ "message": "..." }`. Padronize toasts/dialogs lendo esse campo e trate `401` forçando logout.
4. **Formulários**: como enums trafegam como strings, use selects com os valores das enumerações listadas acima e envie exatamente os literais esperados.
5. **Listagens**: a API não retorna metadados de paginação; mantenha `page`/`pageSize` no estado do componente e trate o array vazio como fim da lista. Considere implementar paginação infinita com `page++` após respostas não vazias.
6. **Multi-step flows**: criação de contas financeiras ou histórias depende de relacionamentos (clientes, fornecedores, produtos). Garanta que o front valide a existência dos IDs antes de enviar para evitar `404`.
7. **Uploads/Anexos**: limite o tamanho localmente antes de enviar `multipart` para `/api/tickets/{id}/anexos` e apresente feedback de progresso, pois o backend lê o arquivo inteiro em memória.
8. **Sincronização Kanban/Tickets**: após adicionar movimentações ou respostas, use o objeto retornado (história/ticket completo) para atualizar o estado local e evitar novas requisições.

---
Este markdown cobre todos os controladores expostos na solução Elo, com foco em rotas, payloads e estruturas de resposta para apoiar tanto a API quanto o consumo pelo front-end.

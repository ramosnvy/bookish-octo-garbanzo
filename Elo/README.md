# API Elo

API desenvolvida seguindo os padrões SOLID, Clean Architecture e CQRS com .NET 8, Entity Framework Core e PostgreSQL.

## Tecnologias Utilizadas

- .NET 8
- Entity Framework Core 8.0
- PostgreSQL
- JWT Authentication
- MediatR (CQRS)
- BCrypt para hash de senhas
- Swagger/OpenAPI

## Estrutura do Projeto

```
Elo/
├── Domain/                 # Camada de Domínio
│   ├── Entities/          # Entidades do domínio
│   ├── Enums/            # Enumeradores
│   └── ValueObjects/     # Objetos de valor
├── Application/           # Camada de Aplicação
│   ├── Commands/         # Comandos CQRS
│   ├── Queries/          # Consultas CQRS
│   ├── Handlers/         # Handlers para Commands e Queries
│   ├── DTOs/             # Data Transfer Objects
│   ├── Interfaces/       # Interfaces da aplicação
│   └── Validators/       # Validadores
├── Infrastructure/        # Camada de Infraestrutura
│   ├── Data/            # DbContext e configurações
│   ├── Repositories/     # Implementações dos repositórios
│   ├── Services/        # Serviços de infraestrutura
│   └── Middleware/      # Middlewares customizados
└── Presentation/         # Camada de Apresentação
    └── Controllers/      # Controllers da API
```

## Configuração

### 1. Banco de Dados

Configure a string de conexão no `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=EloDB;Username=postgres;Password=postgres"
  }
}
```

### 2. JWT Configuration

Configure as chaves JWT no `appsettings.json`:

```json
{
  "Jwt": {
    "SecretKey": "MinhaChaveSecretaSuperSeguraParaJWT2024!@#",
    "Issuer": "EloAPI",
    "Audience": "EloClient",
    "ExpirationMinutes": 60
  }
}
```

### 3. Executar a Aplicação

```bash
cd Elo/Elo
dotnet restore
dotnet run
```

A API estará disponível em `https://localhost:7000` (ou porta configurada).

## Endpoints Disponíveis

### Autenticação

- `POST /api/auth/login` - Login de usuário
- `POST /api/auth/register` - Registro de usuário

### Clientes

- `GET /api/clientes` - Listar todos os clientes
- `GET /api/clientes/{id}` - Obter cliente por ID
- `POST /api/clientes` - Criar novo cliente
- `PUT /api/clientes/{id}` - Atualizar cliente
- `DELETE /api/clientes/{id}` - Deletar cliente

## Modelos de Dados

### Users
- Id, Nome, Email, PasswordHash, Role

### Clientes
- Id, Nome, CnpjCpf, Email, Telefone, Status, DataCadastro

### Fornecedores
- Id, Nome, Cnpj, Email, Telefone, Categoria, Status, DataCadastro

### Produtos
- Id, Nome, Descricao, ValorCusto, ValorRevenda, MargemLucro, Ativo

### Implantacoes
- Id, ClienteId, ProdutoId, Status, UsuarioResponsavelId, DataInicio, DataFinalizacao, Observacoes

### Movimentacoes
- Id, ImplantacaoId, StatusAnterior, StatusNovo, UsuarioId, DataMovimentacao

### Tickets
- Id, ClienteId, Titulo, Descricao, Tipo, Prioridade, Status, UsuarioAtribuidoId, DataAbertura, DataFechamento

### RespostasTicket
- Id, TicketId, UsuarioId, Mensagem, DataResposta

### ContasReceber
- Id, ClienteId, Descricao, Valor, DataVencimento, DataRecebimento, Status, FormaPagamento

### ContasPagar
- Id, FornecedorId, Descricao, Valor, DataVencimento, DataPagamento, Status, Categoria

## Exemplos de Uso

### Login

```bash
curl -X POST "https://localhost:7000/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@elo.com",
    "password": "123456"
  }'
```

### Criar Cliente

```bash
curl -X POST "https://localhost:7000/api/clientes" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer SEU_JWT_TOKEN" \
  -d '{
    "nome": "Cliente Exemplo",
    "cnpjCpf": "12345678901",
    "email": "cliente@exemplo.com",
    "telefone": "11999999999",
    "status": "Ativo"
  }'
```

## Padrões Implementados

### SOLID
- **S**ingle Responsibility: Cada classe tem uma única responsabilidade
- **O**pen/Closed: Aberto para extensão, fechado para modificação
- **L**iskov Substitution: Substituição de implementações
- **I**nterface Segregation: Interfaces específicas e coesas
- **D**ependency Inversion: Dependência de abstrações, não implementações

### Clean Architecture
- Separação clara entre camadas
- Dependências apontam para dentro
- Domínio independente de frameworks

### CQRS
- Separação entre Commands (escrita) e Queries (leitura)
- Handlers específicos para cada operação
- Uso do MediatR para desacoplamento

## Próximos Passos

1. Implementar controllers para as demais entidades
2. Adicionar validações com FluentValidation
3. Implementar paginação
4. Adicionar logs estruturados
5. Implementar testes unitários
6. Adicionar documentação Swagger mais detalhada

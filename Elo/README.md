# API Elo

API multi-empresa construída com .NET 8, ASP.NET Core Web API, PostgreSQL e CQRS/MediatR. O projeto segue princípios de Clean Architecture e SOLID, mantendo serviços de domínio isolados e casos de uso (Commands/Queries + Handler) consolidados por arquivo.

## Stack principal

- .NET 8 / ASP.NET Core Web API
- Entity Framework Core 8 (Npgsql)
- MediatR + CQRS
- FluentValidation (pipeline de validação já configurado)
- JWT + BCrypt
- Swagger (habilitado em Development)
- Docker Compose para banco PostgreSQL

## Arquitetura e organização

```
Elo/
├── Domain/                     # Regras de domínio puras
│   ├── Entities/               # Empresa, Pessoa (Cliente/Fornecedor), Produto, Contas, Tickets etc.
│   ├── Enums/                  # Status/roles/enums de negócio
│   ├── Exceptions/             # Exceções de domínio
│   ├── Interfaces/             # Portas de domínio (serviços e repositórios)
│   ├── Services/               # Serviços de domínio (Pessoa, User, Produto...)
│   └── ValueObjects/           # Reservado (ainda sem objetos de valor)
├── Application/                # Orquestração de casos de uso
│   ├── UseCases/               # Command/Query + Handler no mesmo arquivo
│   │   ├── Auth/               # Login, Register, Me
│   │   ├── Users/              # CRUD + troca de senha
│   │   ├── Empresas/           # Administração de empresas
│   │   ├── Clientes/           # CRUD
│   │   ├── Fornecedores/       # CRUD
│   │   ├── FornecedorCategorias/ # CRUD
│   │   ├── Produtos/           # CRUD + cálculo de margem
│   │   ├── ContasReceber/      # CRUD + mudança de status de parcela
│   │   └── ContasPagar/        # CRUD + visão de calendário
│   ├── DTOs/                   # Modelos expostos pela API
│   ├── Interfaces/             # Serviços de aplicação (JWT, contexto da empresa)
│   ├── Mappers/                # Tradução entidade ⇄ DTO
│   ├── Services/               # Serviços de aplicação
│   └── Validators/             # Validadores (pipeline do MediatR)
├── Infrastructure/             # Adaptadores
│   ├── Configuration/          # Swagger, etc.
│   ├── Data/                   # DbContext, migrations, seed
│   ├── Middleware/             # Tratamento global de exceção
│   ├── Repositories/           # Implementação de UoW/repos
│   └── Services/               # JWT, contexto de empresa, etc.
└── Presentation/               # Borda HTTP
    └── Controllers/            # Endpoints agrupados por domínio
```

## Configuração e execução

1. Pré-requisitos: .NET 8 SDK, Docker (para subir o PostgreSQL) e acesso à porta 5432.
2. Suba o banco local (opcional, se já tiver um PostgreSQL disponível):
   ```bash
   cd Elo
   docker-compose up -d
   ```
3. Ajuste `Elo/appsettings.json` (ou `appsettings.Development.json`) com a string de conexão e chaves JWT. O padrão aponta para `Host=localhost;Port=5432;Database=elo;Username=elo;Password=elo123`.
4. Restaure/rode a aplicação:
   ```bash
   dotnet restore Elo/Elo.Api.csproj
   dotnet run --project Elo/Elo.Api.csproj --launch-profile https
   ```
   - Perfis expõem por padrão `https://localhost:7271` e `http://localhost:5172` (Swagger habilitado em Development).
   - O `DbInitializer` cria um usuário admin se o banco estiver vazio: `admin@elo.com` / `123456` (sem empresa associada = "admin global").

> Observação: a aplicação chama `EnsureCreated` no startup. Caso queira evoluir o schema via migrations, rode `dotnet ef database update` a partir da pasta `Elo` para aplicar as migrations já versionadas.

## Autenticação e escopo de empresa

- Autenticação via Bearer JWT; o token inclui `companyId` quando o usuário pertence a uma empresa.
- Usuário sem `companyId` e com role `Admin` é tratado como admin global e pode agir em qualquer empresa passando `?empresaId=` nas rotas que suportam o parâmetro.
- Demais usuários operam somente na empresa do token (resolvida pelo `IEmpresaContextService`).

## Endpoints implementados

- **Auth (`/api/auth`)**: `POST login`, `POST register`, `GET me` (autenticado), `POST logout` (stateless).
- **Empresas (`/api/empresas`, Admin)**: `GET`, `POST`, `PUT` (cria empresa e usuário inicial).
- **Usuários (`/api/users`)**: `GET`, `GET /{id}`, `POST`, `PUT /{id}`, `DELETE /{id}`, `PUT /{id}/change-password`.
- **Clientes (`/api/clientes`)**: `GET`, `GET /{id}`, `POST`, `PUT /{id}`, `DELETE /{id}` (endereços incluídos).
- **Fornecedores (`/api/fornecedores`)**: `GET`, `GET /{id}`, `POST`, `PUT /{id}`, `DELETE /{id}` (categoria, condições de pagamento, endereços).
- **Categorias de fornecedores (`/api/fornecedorcategorias`)**: `GET`, `POST`, `PUT /{id}`, `DELETE /{id}`.
- **Produtos (`/api/produtos`)**: `GET`, `GET /{id}`, `POST`, `PUT /{id}`, `DELETE /{id}`, `POST /calcular-margem` (cálculo simples de margem).
- **Histórias (`/api/historias`)**: `GET` com filtros de status/tipo/cliente/produto/responsável, `GET /{id}`, `POST`, `PUT /{id}`, `DELETE /{id}`, `POST /{id}/movimentacoes` (atualiza status e histórico).
- **Tickets (`/api/tickets`)**: `GET` com filtros, `GET /{id}`, `POST`, `PUT /{id}`, `DELETE /{id}`, `POST /{id}/respostas` (respostas internas/externas).
- **Financeiro - Contas a Receber (`/api/financeiro/contas-receber`)**: `GET` com filtros de status/período, `GET /{id}`, `POST`, `PUT /{id}`, `DELETE /{id}`, `PUT /{contaId}/parcelas/{parcelaId}/status`.
- **Financeiro - Contas a Pagar (`/api/financeiro/contas-pagar`)**: `GET` com filtros de status/período, `GET /calendario` (eventos para calendário), `GET /{id}`, `POST`, `PUT /{id}`, `DELETE /{id}`.

## Lacunas e pendências mapeadas

- Paginação, busca e filtros (`page`, `pageSize`, `search`, etc.) estão aceitos nas rotas de usuários, clientes, fornecedores e produtos, mas ainda não são aplicados nos handlers (retornam lista completa).
- Validações FluentValidation estão configuradas no pipeline, porém só existe validador para criação de cliente; demais comandos/DTOs ainda sem validação específica.
- `Domain/ValueObjects` e eventos de domínio ainda não foram implementados (pastas vazias).
- Testes automatizados (unitários/integração) e logging estruturado ainda não existem no repositório.
- `POST /api/auth/register` não associa o usuário a uma empresa; o vínculo só é feito via CRUD de usuários (pode ser um ajuste necessário para onboarding).

## Exemplo rápido (login)

```bash
curl -X POST "https://localhost:7271/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"email": "admin@elo.com", "password": "123456"}'
```

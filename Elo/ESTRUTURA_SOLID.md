# Estrutura da API Elo - Seguindo Princípios SOLID

## Estrutura Atual vs Estrutura Melhorada

### ❌ Estrutura Anterior (Violando SOLID)

```
Elo/
├── Domain/
│   ├── Entities/          # Entidades do domínio
│   └── Enums/            # Enumeradores
├── Application/
│   ├── Commands/         # Comandos CQRS
│   ├── Queries/          # Consultas CQRS
│   ├── Handlers/         # Handlers (fazendo TUDO)
│   ├── DTOs/             # Data Transfer Objects
│   └── Interfaces/       # Interfaces da aplicação
├── Infrastructure/
│   ├── Data/            # DbContext
│   ├── Repositories/     # Repositórios
│   └── Services/        # Serviços de infraestrutura
└── Presentation/
    └── Controllers/      # Controllers
```

### ✅ Estrutura Melhorada (Seguindo SOLID)

```
Elo/
├── Domain/                    # 🎯 CAMADA DE DOMÍNIO
│   ├── Entities/             # Entidades do domínio
│   ├── Enums/               # Enumeradores
│   ├── ValueObjects/        # Objetos de valor
│   ├── Interfaces/          # Interfaces do domínio (DIP)
│   ├── Services/            # Serviços de domínio (SRP)
│   └── Exceptions/          # Exceções de domínio
├── Application/              # 🎯 CAMADA DE APLICAÇÃO
│   ├── Commands/            # Comandos CQRS
│   ├── Queries/             # Consultas CQRS
│   ├── Handlers/            # Handlers (apenas orquestração)
│   ├── DTOs/                # Data Transfer Objects
│   ├── Interfaces/          # Interfaces da aplicação
│   ├── Services/            # Serviços de aplicação
│   ├── Mappers/             # Mapeadores (SRP)
│   ├── Validators/          # Validadores (SRP)
│   └── Behaviors/           # Behaviors do MediatR
├── Infrastructure/           # 🎯 CAMADA DE INFRAESTRUTURA
│   ├── Data/               # DbContext e configurações
│   ├── Repositories/        # Implementações dos repositórios
│   ├── Services/           # Serviços de infraestrutura
│   ├── Middleware/         # Middlewares customizados
│   └── Configuration/      # Configurações
└── Presentation/            # 🎯 CAMADA DE APRESENTAÇÃO
    ├── Controllers/         # Controllers da API
    ├── Middleware/         # Middlewares de apresentação
    └── Filters/            # Filtros de ação
```

## Como os Princípios SOLID Foram Aplicados

### 1. **Single Responsibility Principle (SRP)** ✅

**Antes (Violando SRP):**
```csharp
public class CreateClienteCommandHandler : IRequestHandler<CreateClienteCommand, ClienteDto>
{
    public async Task<ClienteDto> Handle(CreateClienteCommand request, CancellationToken cancellationToken)
    {
        // ❌ Validação de negócio
        var existingCliente = await _unitOfWork.Clientes.FirstOrDefaultAsync(c => 
            c.Email == request.Email || c.CnpjCpf == request.CnpjCpf);

        // ❌ Criação da entidade
        var cliente = new Cliente { ... };

        // ❌ Persistência
        await _unitOfWork.Clientes.AddAsync(cliente);
        await _unitOfWork.SaveChangesAsync();

        // ❌ Mapeamento
        return new ClienteDto { ... };
    }
}
```

**Depois (Seguindo SRP):**
```csharp
public class CreateClienteCommandHandler : IRequestHandler<CreateClienteCommand, ClienteDto>
{
    private readonly IClienteService _clienteService;  // Responsabilidade: Lógica de negócio
    private readonly IClienteMapper _clienteMapper;    // Responsabilidade: Mapeamento

    public async Task<ClienteDto> Handle(CreateClienteCommand request, CancellationToken cancellationToken)
    {
        // ✅ Delegação da lógica de negócio
        var cliente = await _clienteService.CriarClienteAsync(...);

        // ✅ Delegação do mapeamento
        return _clienteMapper.ToDto(cliente);
    }
}
```

**Separação de Responsabilidades:**
- **Handler**: Apenas orquestração
- **ClienteService**: Lógica de negócio
- **ClienteMapper**: Mapeamento de objetos
- **Validator**: Validação de dados
- **Repository**: Persistência de dados

### 2. **Open/Closed Principle (OCP)** ✅

**Antes (Violando OCP):**
```csharp
// Para adicionar validação, precisava modificar o handler
public class CreateClienteCommandHandler
{
    public async Task<ClienteDto> Handle(CreateClienteCommand request, CancellationToken cancellationToken)
    {
        // Validação hardcoded no handler
        if (string.IsNullOrEmpty(request.Nome))
            throw new ArgumentException("Nome é obrigatório");
    }
}
```

**Depois (Seguindo OCP):**
```csharp
// ✅ Aberto para extensão, fechado para modificação
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    // Validação genérica que funciona para qualquer comando
}

public class CreateClienteCommandValidator : AbstractValidator<CreateClienteCommand>
{
    // Validação específica sem modificar o handler
}
```

### 3. **Liskov Substitution Principle (LSP)** ✅

**Implementação:**
```csharp
// ✅ Qualquer implementação de IClienteService pode ser substituída
public interface IClienteService
{
    Task<Cliente> CriarClienteAsync(...);
}

public class ClienteService : IClienteService { ... }
public class ClienteServiceMock : IClienteService { ... }  // Para testes
public class ClienteServiceCached : IClienteService { ... } // Com cache
```

### 4. **Interface Segregation Principle (ISP)** ✅

**Antes (Violando ISP):**
```csharp
// ❌ Interface muito grande
public interface IRepository<T>
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    // ... muitos outros métodos
}
```

**Depois (Seguindo ISP):**
```csharp
// ✅ Interfaces específicas e coesas
public interface IClienteService
{
    Task<Cliente> CriarClienteAsync(...);
    Task<Cliente> AtualizarClienteAsync(...);
    Task<bool> DeletarClienteAsync(int id);
}

public interface IClienteMapper
{
    ClienteDto ToDto(Cliente cliente);
    IEnumerable<ClienteDto> ToDtoList(IEnumerable<Cliente> clientes);
}
```

### 5. **Dependency Inversion Principle (DIP)** ✅

**Antes (Violando DIP):**
```csharp
// ❌ Dependência de implementação concreta
public class CreateClienteCommandHandler
{
    private readonly IUnitOfWork _unitOfWork;  // Dependência de infraestrutura
}
```

**Depois (Seguindo DIP):**
```csharp
// ✅ Dependência de abstrações
public class CreateClienteCommandHandler
{
    private readonly IClienteService _clienteService;  // Dependência de domínio
    private readonly IClienteMapper _clienteMapper;    // Dependência de aplicação
}
```

## Fluxo de Dados na Arquitetura Melhorada

```
Controller → Command/Query → Handler → Domain Service → Repository → Database
    ↓              ↓           ↓            ↓            ↓
   DTOs        Validation   Mapper    Business Logic   Data Access
```

### Exemplo Prático: Criar Cliente

1. **Controller** recebe `CreateClienteDto`
2. **Command** é criado com os dados
3. **Validator** valida os dados (Behavior automático)
4. **Handler** orquestra o processo
5. **Domain Service** executa lógica de negócio
6. **Repository** persiste os dados
7. **Mapper** converte entidade para DTO
8. **Controller** retorna `ClienteDto`

## Benefícios da Estrutura Melhorada

### ✅ **Testabilidade**
- Cada classe tem uma responsabilidade específica
- Fácil de criar mocks e stubs
- Testes unitários isolados

### ✅ **Manutenibilidade**
- Mudanças em uma responsabilidade não afetam outras
- Código mais limpo e organizado
- Fácil de entender e modificar

### ✅ **Extensibilidade**
- Novas funcionalidades sem modificar código existente
- Behaviors do MediatR para cross-cutting concerns
- Interfaces bem definidas

### ✅ **Reutilização**
- Serviços de domínio podem ser reutilizados
- Mappers podem ser usados em diferentes contextos
- Validações centralizadas

### ✅ **Separação de Responsabilidades**
- Domínio independente de frameworks
- Lógica de negócio isolada
- Infraestrutura desacoplada

## Próximos Passos

1. **Implementar Value Objects** para encapsular regras de negócio
2. **Adicionar Domain Events** para comunicação entre agregados
3. **Implementar Specification Pattern** para consultas complexas
4. **Adicionar CQRS com Read Models** separados
5. **Implementar Unit of Work** com transações
6. **Adicionar Logging e Monitoring** estruturado

## Conclusão

A estrutura melhorada segue rigorosamente os princípios SOLID, resultando em:
- **Código mais limpo e organizado**
- **Fácil manutenção e extensão**
- **Alta testabilidade**
- **Separação clara de responsabilidades**
- **Arquitetura escalável e robusta**

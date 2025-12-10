# Plano de Validação de Status

## Objetivo
Garantir que todas as entidades com status sejam respeitadas em todas as operações do sistema.

## Entidades com Status

### 1. **User** (Usuário)
- **Campo**: Não possui campo de status atualmente ❌
- **Ação**: Adicionar campo `Status` do tipo `Status` enum
- **Validações necessárias**:
  - Login: Usuários inativos não devem conseguir fazer login
  - Atribuição: Usuários inativos não devem ser atribuídos a tickets/histórias

### 2. **Empresa**
- **Campo**: `Ativo` (bool) ✅
- **Validações necessárias**:
  - Login: Usuários de empresas inativas não devem conseguir fazer login
  - Operações: Todas as operações devem verificar se a empresa está ativa
  - Criação de registros: Não permitir criar registros para empresas inativas

### 3. **Pessoa** (Cliente/Fornecedor)
- **Campo**: `Status` (Status enum) ✅
- **Validações necessárias**:
  - Histórias: Não permitir criar histórias para clientes inativos
  - Tickets: Não permitir criar tickets para clientes inativos
  - Contas a Receber: Não permitir criar contas para clientes inativos
  - Contas a Pagar: Não permitir criar contas para fornecedores inativos
  - Assinaturas: Não permitir criar assinaturas para clientes inativos

### 4. **Produto**
- **Campo**: `Ativo` (bool) ✅
- **Validações necessárias**:
  - Histórias: Não permitir criar histórias com produtos inativos
  - Assinaturas: Não permitir adicionar produtos inativos em assinaturas
  - Tickets: Avisar se produto está inativo ao criar ticket

### 5. **ProdutoModulo**
- **Campo**: `Ativo` (bool) ✅
- **Validações necessárias**:
  - Histórias: Ao criar história, validar se módulos selecionados estão ativos
  - Assinaturas: Validar se módulos estão ativos

### 6. **FornecedorCategoria**
- **Campo**: `Ativo` (bool) ✅
- **Validações necessárias**:
  - Já validado no PessoaService ✅

### 7. **Assinatura**
- **Campo**: `Ativo` (bool) ✅
- **Validações necessárias**:
  - Geração de financeiro: Apenas assinaturas ativas devem gerar financeiro
  - Listagens: Filtrar por status quando apropriado

## Implementações Necessárias

### Fase 1: Adicionar Status ao User
1. Adicionar campo `Status` à entidade `User`
2. Criar migration
3. Atualizar DTOs e Use Cases

### Fase 2: Validações de Login
1. `UserService.ValidarCredenciaisAsync`: Verificar status do usuário
2. `UserService.ValidarCredenciaisAsync`: Verificar se empresa está ativa
3. `Login.Handler`: Adicionar validações

### Fase 3: Validações em Serviços de Domínio

#### HistoriaService
- `CriarHistoriaAsync`: Validar cliente ativo
- `CriarHistoriaAsync`: Validar produto ativo
- `CriarHistoriaAsync`: Validar módulos ativos (se aplicável)

#### TicketService
- `CriarTicketAsync`: Validar cliente ativo
- `CriarTicketAsync`: Validar produto ativo (se informado)
- `CriarTicketAsync`: Validar fornecedor ativo (se informado)

#### ContaReceberService
- `CriarContaAsync`: Validar cliente ativo

#### ContaPagarService
- `CriarContaAsync`: Validar fornecedor ativo

#### AssinaturaService
- `CriarAssinaturaAsync`: Validar cliente ativo
- `CriarAssinaturaAsync`: Validar produtos ativos
- `CriarAssinaturaAsync`: Validar módulos ativos

#### ProdutoService
- `ObterFornecedorValidoAsync`: Validar fornecedor ativo ✅ (já valida categoria)

### Fase 4: Validações em Atribuições
- Histórias: Validar usuário responsável ativo
- Tickets: Validar usuário atribuído ativo

### Fase 5: Criar Exceções Específicas
- `UsuarioInativoException`
- `EmpresaInativaException`
- `ClienteInativoException`
- `FornecedorInativoException`
- `ProdutoInativoException`
- `ModuloInativoException`

## Prioridade de Implementação

1. **Alta**: Login (User + Empresa)
2. **Alta**: Criação de Histórias e Assinaturas (Cliente + Produto)
3. **Média**: Tickets (Cliente + Fornecedor + Produto)
4. **Média**: Financeiro (Cliente + Fornecedor)
5. **Baixa**: Atribuições (Usuário)

## Notas
- Considerar se devemos bloquear completamente ou apenas avisar em alguns casos
- Avaliar se registros existentes devem ser afetados (ex: histórias abertas de produtos que ficaram inativos)
- Definir comportamento para soft delete vs inativação

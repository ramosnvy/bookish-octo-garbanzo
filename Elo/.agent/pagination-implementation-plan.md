# Plano de Implementação: Paginação e Otimização de Endpoints

## Objetivo
Implementar paginação e criar endpoints simplificados/detalhados para melhorar a performance do front-end.

## Problemas Identificados
1. Endpoints retornam informações além das necessárias
2. Sem paginação, causando lentidão no front-end
3. Necessidade de endpoints simplificados para listagens e detalhados para visualização individual

## Solução Proposta

### 1. Criar DTOs Simplificados
- **HistoriaListDto**: Versão simplificada sem movimentações e produtos
- **TicketListDto**: Versão simplificada sem respostas e anexos
- **ClienteListDto**: Versão simplificada sem endereços completos
- **ProdutoListDto**: Versão simplificada sem módulos completos

### 2. Implementar Sistema de Paginação
- **PagedResult<T>**: Classe genérica para resultados paginados
- Incluir: Items, TotalCount, PageNumber, PageSize, TotalPages, HasPrevious, HasNext

### 3. Atualizar Use Cases
- Modificar `GetAllHistorias`, `GetAllTickets`, `GetAllClientes`, etc.
- Retornar `PagedResult<TListDto>` ao invés de `IEnumerable<TDto>`
- Implementar queries otimizadas (sem includes desnecessários)

### 4. Atualizar Controllers
- Manter endpoints detalhados: `GET /api/{resource}/{id}` → retorna DTO completo
- Otimizar endpoints de listagem: `GET /api/{resource}` → retorna PagedResult<ListDto>
- Adicionar parâmetros de paginação: page, pageSize

### 5. Ajustar Front-end
- Implementar componente de paginação
- Atualizar chamadas de API para incluir parâmetros de paginação
- Tratar resposta paginada

## Recursos Prioritários
1. Historias (mais complexo, com movimentações e produtos)
2. Tickets (com respostas e anexos)
3. Clientes (com endereços)
4. Produtos (com módulos)
5. Fornecedores
6. Usuários

## Estrutura de Implementação

### Backend
```
Elo.Application/
├── Common/
│   └── PagedResult.cs
├── DTOs/
│   ├── Historia/
│   │   └── HistoriaListDto.cs
│   ├── Ticket/
│   │   └── TicketListDto.cs
│   └── ...
└── UseCases/
    ├── Historias/
    │   └── GetAllHistorias.cs (atualizado)
    └── ...
```

### Frontend
```
src/
├── components/
│   └── Pagination.tsx
├── services/
│   └── api.ts (atualizado)
└── types/
    └── pagination.ts
```

## Status
- [ ] Criar PagedResult<T>
- [ ] Criar DTOs simplificados
- [ ] Atualizar Use Cases
- [ ] Atualizar Controllers
- [ ] Ajustar Front-end

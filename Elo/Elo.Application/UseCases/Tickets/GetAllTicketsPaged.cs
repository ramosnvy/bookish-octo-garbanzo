using MediatR;
using Elo.Application.Common;
using Elo.Application.DTOs.Ticket;
using Elo.Domain.Interfaces;
using Elo.Domain.Interfaces.Repositories;
using Elo.Domain.Enums;

namespace Elo.Application.UseCases.Tickets;

public static class GetAllTicketsPaged
{
    public class Query : IRequest<PagedResult<TicketListDto>>
    {
        public int? EmpresaId { get; set; }
        public TicketStatus? Status { get; set; }
        public int? TipoId { get; set; }
        public TicketPrioridade? Prioridade { get; set; }
        public int? ClienteId { get; set; }
        public int? ProdutoId { get; set; }
        public int? FornecedorId { get; set; }
        public int? UsuarioAtribuidoId { get; set; }
        public DateTime? DataAberturaInicio { get; set; }
        public DateTime? DataAberturaFim { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }

    public class Handler : IRequestHandler<Query, PagedResult<TicketListDto>>
    {
        private readonly ITicketService _ticketService;
        private readonly IUnitOfWork _unitOfWork;

        public Handler(ITicketService ticketService, IUnitOfWork unitOfWork)
        {
            _ticketService = ticketService;
            _unitOfWork = unitOfWork;
        }

        public async Task<PagedResult<TicketListDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            // Validar parâmetros de paginação
            var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
            var pageSize = request.PageSize < 1 ? 10 : (request.PageSize > 100 ? 100 : request.PageSize);

            var tickets = await _ticketService.ObterTicketsAsync(request.EmpresaId, request.ClienteId, request.Status);
            
            // Aplicar filtros adicionais
            if (request.TipoId.HasValue)
                tickets = tickets.Where(t => t.TicketTipoId == request.TipoId.Value);
            
            if (request.Prioridade.HasValue)
                tickets = tickets.Where(t => t.Prioridade == request.Prioridade.Value);
            
            if (request.ProdutoId.HasValue)
                tickets = tickets.Where(t => t.ProdutoId == request.ProdutoId.Value);
            
            if (request.FornecedorId.HasValue)
                tickets = tickets.Where(t => t.FornecedorId == request.FornecedorId.Value);
            
            if (request.UsuarioAtribuidoId.HasValue)
                tickets = tickets.Where(t => t.UsuarioAtribuidoId == request.UsuarioAtribuidoId.Value);
            
            if (request.DataAberturaInicio.HasValue)
                tickets = tickets.Where(t => t.DataAbertura >= request.DataAberturaInicio.Value);
            
            if (request.DataAberturaFim.HasValue)
                tickets = tickets.Where(t => t.DataAbertura <= request.DataAberturaFim.Value);

            var ticketsList = tickets.ToList();
            var totalCount = ticketsList.Count;

            if (totalCount == 0)
                return PagedResult<TicketListDto>.Empty(pageNumber, pageSize);

            // Apply pagination
            var pagedList = ticketsList
                .OrderByDescending(t => t.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
            
            // Carregar dados relacionados apenas para a página atual
            var clientesIds = pagedList.Select(t => t.ClienteId).Distinct().ToList();
            var clientes = await _unitOfWork.Pessoas.FindAsync(p => clientesIds.Contains(p.Id));
            var clienteLookup = clientes.ToDictionary(c => c.Id, c => c.Nome);

            var tiposIds = pagedList.Select(t => t.TicketTipoId).Distinct().ToList();
            var tipos = await _unitOfWork.TicketTipos.FindAsync(t => tiposIds.Contains(t.Id));
            var tipoLookup = tipos.ToDictionary(t => t.Id, t => t.Nome);

            var usuariosIds = pagedList.Where(t => t.UsuarioAtribuidoId.HasValue).Select(t => t.UsuarioAtribuidoId!.Value).Distinct().ToList();
            var usuarios = usuariosIds.Any() 
                ? await _unitOfWork.Users.FindAsync(u => usuariosIds.Contains(u.Id)) 
                : Enumerable.Empty<Domain.Entities.User>();
            var usuarioLookup = usuarios.ToDictionary(u => u.Id, u => u.Nome);

            var produtosIds = pagedList.Where(t => t.ProdutoId.HasValue).Select(t => t.ProdutoId!.Value).Distinct().ToList();
            var produtos = produtosIds.Any() 
                ? await _unitOfWork.Produtos.FindAsync(p => produtosIds.Contains(p.Id)) 
                : Enumerable.Empty<Domain.Entities.Produto>();
            var produtoLookup = produtos.ToDictionary(p => p.Id, p => p.Nome);

            var fornecedoresIds = pagedList.Where(t => t.FornecedorId.HasValue).Select(t => t.FornecedorId!.Value).Distinct().ToList();
            var fornecedores = fornecedoresIds.Any() 
                ? await _unitOfWork.Pessoas.FindAsync(p => fornecedoresIds.Contains(p.Id)) 
                : Enumerable.Empty<Domain.Entities.Pessoa>();
            var fornecedorLookup = fornecedores.ToDictionary(f => f.Id, f => f.Nome);

            // Contar respostas e anexos para cada ticket
            var ticketIds = pagedList.Select(t => t.Id).ToList();
            var respostas = await _unitOfWork.RespostasTicket.FindAsync(r => ticketIds.Contains(r.TicketId));
            var respostasCount = respostas.GroupBy(r => r.TicketId).ToDictionary(g => g.Key, g => g.Count());
            
            var anexos = await _unitOfWork.TicketAnexos.FindAsync(a => ticketIds.Contains(a.TicketId));
            var anexosCount = anexos.GroupBy(a => a.TicketId).ToDictionary(g => g.Key, g => g.Count());

            // Map to DTOs
            var items = pagedList.Select(t => new TicketListDto
            {
                Id = t.Id,
                Titulo = t.Titulo,
                Descricao = t.Descricao,
                Status = t.Status,
                StatusNome = t.Status.ToString(),
                Prioridade = t.Prioridade,
                PrioridadeNome = t.Prioridade.ToString(),
                TipoId = t.TicketTipoId,
                TipoNome = tipoLookup.GetValueOrDefault(t.TicketTipoId, string.Empty),
                ClienteId = t.ClienteId,
                ClienteNome = clienteLookup.GetValueOrDefault(t.ClienteId, string.Empty),
                ProdutoId = t.ProdutoId,
                ProdutoNome = t.ProdutoId.HasValue ? produtoLookup.GetValueOrDefault(t.ProdutoId.Value, string.Empty) : null,
                FornecedorId = t.FornecedorId,
                FornecedorNome = t.FornecedorId.HasValue ? fornecedorLookup.GetValueOrDefault(t.FornecedorId.Value, string.Empty) : null,
                UsuarioAtribuidoId = t.UsuarioAtribuidoId,
                UsuarioAtribuidoNome = t.UsuarioAtribuidoId.HasValue ? usuarioLookup.GetValueOrDefault(t.UsuarioAtribuidoId.Value, string.Empty) : null,
                DataAbertura = t.DataAbertura,
                DataFechamento = t.DataFechamento,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                QuantidadeRespostas = respostasCount.GetValueOrDefault(t.Id, 0),
                QuantidadeAnexos = anexosCount.GetValueOrDefault(t.Id, 0)
            }).ToList();

            return new PagedResult<TicketListDto>(items, totalCount, pageNumber, pageSize);
        }
    }
}

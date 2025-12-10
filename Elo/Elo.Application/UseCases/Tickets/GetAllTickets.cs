using MediatR;
using Elo.Application.DTOs.Ticket;
using Elo.Domain.Interfaces;
using Elo.Domain.Interfaces.Repositories;
using Elo.Domain.Entities;
using Elo.Domain.Enums;

namespace Elo.Application.UseCases.Tickets;

public static class GetAllTickets
{
    public class Query : IRequest<IEnumerable<TicketDto>>
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
    }

    public class Handler : IRequestHandler<Query, IEnumerable<TicketDto>>
    {
        private readonly ITicketService _ticketService;
        private readonly IUnitOfWork _unitOfWork;

        public Handler(ITicketService ticketService, IUnitOfWork unitOfWork)
        {
            _ticketService = ticketService;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<TicketDto>> Handle(Query request, CancellationToken cancellationToken)
        {
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
            
            // Carregar dados relacionados
            var clientesIds = ticketsList.Select(t => t.ClienteId).Distinct().ToList();
            var clientes = await _unitOfWork.Pessoas.FindAsync(p => clientesIds.Contains(p.Id));
            var clienteLookup = clientes.ToDictionary(c => c.Id);

            var tiposIds = ticketsList.Select(t => t.TicketTipoId).Distinct().ToList();
            var tipos = await _unitOfWork.TicketTipos.FindAsync(t => tiposIds.Contains(t.Id));
            var tipoLookup = tipos.ToDictionary(t => t.Id);

            var usuariosIds = ticketsList.Where(t => t.UsuarioAtribuidoId.HasValue).Select(t => t.UsuarioAtribuidoId!.Value).Distinct().ToList();
            var usuarios = usuariosIds.Any() ? await _unitOfWork.Users.FindAsync(u => usuariosIds.Contains(u.Id)) : Enumerable.Empty<User>();
            var usuarioLookup = usuarios.ToDictionary(u => u.Id);

            var produtosIds = ticketsList.Where(t => t.ProdutoId.HasValue).Select(t => t.ProdutoId!.Value).Distinct().ToList();
            var produtos = produtosIds.Any() ? await _unitOfWork.Produtos.FindAsync(p => produtosIds.Contains(p.Id)) : Enumerable.Empty<Produto>();
            var produtoLookup = produtos.ToDictionary(p => p.Id);

            var fornecedoresIds = ticketsList.Where(t => t.FornecedorId.HasValue).Select(t => t.FornecedorId!.Value).Distinct().ToList();
            var fornecedores = fornecedoresIds.Any() ? await _unitOfWork.Pessoas.FindAsync(p => fornecedoresIds.Contains(p.Id)) : Enumerable.Empty<Pessoa>();
            var fornecedorLookup = fornecedores.ToDictionary(f => f.Id);

            var respostasLookup = new Dictionary<int, List<RespostaTicket>>();
            var anexosLookup = new Dictionary<int, List<TicketAnexo>>();

            return ticketsList.Select(t => TicketMapper.ToDto(t, clienteLookup, usuarioLookup, respostasLookup, tipoLookup, produtoLookup, fornecedorLookup, anexosLookup));
        }
    }
}

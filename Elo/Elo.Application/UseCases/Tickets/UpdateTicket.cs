using MediatR;
using Elo.Application.DTOs.Ticket;
using Elo.Domain.Interfaces;
using Elo.Domain.Interfaces.Repositories;
using Elo.Domain.Entities;
using Elo.Domain.Enums;

namespace Elo.Application.UseCases.Tickets;

public static class UpdateTicket
{
    public class Command : IRequest<TicketDto>
    {
        public int EmpresaId { get; set; }
        public UpdateTicketDto Dto { get; set; } = new();
    }

    public class Handler : IRequestHandler<Command, TicketDto>
    {
        private readonly ITicketService _ticketService;
        private readonly IUnitOfWork _unitOfWork;

        public Handler(ITicketService ticketService, IUnitOfWork unitOfWork)
        {
            _ticketService = ticketService;
            _unitOfWork = unitOfWork;
        }

        public async Task<TicketDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;
            
            var ticket = await _ticketService.AtualizarTicketAsync(
                dto.Id,
                dto.Titulo,
                dto.Descricao,
                dto.TicketTipoId,
                dto.ProdutoId,
                dto.FornecedorId,
                dto.UsuarioAtribuidoId,
                dto.Status,
                request.EmpresaId
            );

            return await MapearParaDtoAsync(ticket);
        }

        private async Task<TicketDto> MapearParaDtoAsync(Ticket ticket)
        {
            var cliente = await _unitOfWork.Pessoas.GetByIdAsync(ticket.ClienteId);
            var ticketTipo = await _unitOfWork.TicketTipos.GetByIdAsync(ticket.TicketTipoId);
            User? usuario = null;
            if (ticket.UsuarioAtribuidoId.HasValue)
                usuario = await _unitOfWork.Users.GetByIdAsync(ticket.UsuarioAtribuidoId.Value);
            Produto? produto = null;
            if (ticket.ProdutoId.HasValue)
                produto = await _unitOfWork.Produtos.GetByIdAsync(ticket.ProdutoId.Value);
            Pessoa? fornecedor = null;
            if (ticket.FornecedorId.HasValue)
                fornecedor = await _unitOfWork.Pessoas.GetByIdAsync(ticket.FornecedorId.Value);

            var clienteLookup = new Dictionary<int, Pessoa> { { cliente!.Id, cliente } };
            var usuarioLookup = usuario != null ? new Dictionary<int, User> { { usuario.Id, usuario } } : new Dictionary<int, User>();
            var ticketTipoLookup = new Dictionary<int, TicketTipo> { { ticketTipo!.Id, ticketTipo } };
            var produtoLookup = produto != null ? new Dictionary<int, Produto> { { produto.Id, produto } } : new Dictionary<int, Produto>();
            var fornecedorLookup = fornecedor != null ? new Dictionary<int, Pessoa> { { fornecedor.Id, fornecedor } } : new Dictionary<int, Pessoa>();
            var respostasLookup = new Dictionary<int, List<RespostaTicket>> { { ticket.Id, new List<RespostaTicket>() } };
            var anexosLookup = new Dictionary<int, List<TicketAnexo>> { { ticket.Id, new List<TicketAnexo>() } };

            return TicketMapper.ToDto(ticket, clienteLookup, usuarioLookup, respostasLookup, ticketTipoLookup, produtoLookup, fornecedorLookup, anexosLookup);
        }
    }
}

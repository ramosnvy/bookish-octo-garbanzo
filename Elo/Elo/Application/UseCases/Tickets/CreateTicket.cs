using MediatR;
using Elo.Application.DTOs.Ticket;
using Elo.Domain.Entities;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.Tickets;

public static class CreateTicket
{
    public class Command : IRequest<TicketDto>
    {
        public int EmpresaId { get; set; }
        public CreateTicketDto Dto { get; set; } = new();
    }

    public class Handler : IRequestHandler<Command, TicketDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<TicketDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;
            var cliente = await _unitOfWork.Pessoas.GetByIdAsync(dto.ClienteId);
            if (cliente == null || cliente.Tipo != PessoaTipo.Cliente || cliente.EmpresaId != request.EmpresaId)
            {
                throw new KeyNotFoundException("Cliente não encontrado para esta empresa.");
            }

            User? usuarioAtribuido = null;
            if (dto.UsuarioAtribuidoId.HasValue)
            {
                usuarioAtribuido = await _unitOfWork.Users.GetByIdAsync(dto.UsuarioAtribuidoId.Value);
                if (usuarioAtribuido == null || usuarioAtribuido.EmpresaId != request.EmpresaId)
                {
                    throw new KeyNotFoundException("Usuário atribuído não encontrado para esta empresa.");
                }
            }

            var ticket = new Ticket
            {
                ClienteId = dto.ClienteId,
                Titulo = dto.Titulo,
                Descricao = dto.Descricao,
                Tipo = dto.Tipo,
                Prioridade = dto.Prioridade,
                Status = dto.Status,
                UsuarioAtribuidoId = dto.UsuarioAtribuidoId,
                DataAbertura = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            if (dto.Status == TicketStatus.Resolvido || dto.Status == TicketStatus.Fechado || dto.Status == TicketStatus.Cancelado)
            {
                ticket.DataFechamento = DateTime.UtcNow;
            }

            await _unitOfWork.Tickets.AddAsync(ticket);
            await _unitOfWork.SaveChangesAsync();

            var clienteLookup = new Dictionary<int, Pessoa> { { cliente.Id, cliente } };
            var usuarioLookup = usuarioAtribuido != null
                ? new Dictionary<int, User> { { usuarioAtribuido.Id, usuarioAtribuido } }
                : new Dictionary<int, User>();
            var respostasLookup = new Dictionary<int, List<RespostaTicket>>
            {
                { ticket.Id, new List<RespostaTicket>() }
            };

            return TicketMapper.ToDto(ticket, clienteLookup, usuarioLookup, respostasLookup);
        }
    }
}

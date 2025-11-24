using MediatR;
using Elo.Application.DTOs.Ticket;
using Elo.Domain.Entities;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces.Repositories;

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
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<TicketDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;
            var ticket = await _unitOfWork.Tickets.GetByIdAsync(dto.Id);
            if (ticket == null)
            {
                throw new KeyNotFoundException("Ticket não encontrado.");
            }

            var cliente = await _unitOfWork.Pessoas.GetByIdAsync(ticket.ClienteId);
            if (cliente == null || cliente.Tipo != PessoaTipo.Cliente || cliente.EmpresaId != request.EmpresaId)
            {
                throw new UnauthorizedAccessException("Ticket não pertence à empresa informada.");
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
            else
            {
                usuarioAtribuido = null;
            }

            ticket.Titulo = dto.Titulo;
            ticket.Descricao = dto.Descricao;
            ticket.Tipo = dto.Tipo;
            ticket.Prioridade = dto.Prioridade;
            ticket.Status = dto.Status;
            ticket.UsuarioAtribuidoId = dto.UsuarioAtribuidoId;
            ticket.DataFechamento = dto.DataFechamento;
            ticket.UpdatedAt = DateTime.UtcNow;

            if ((dto.Status == TicketStatus.Resolvido || dto.Status == TicketStatus.Fechado || dto.Status == TicketStatus.Cancelado)
                && !ticket.DataFechamento.HasValue)
            {
                ticket.DataFechamento = DateTime.UtcNow;
            }

            await _unitOfWork.Tickets.UpdateAsync(ticket);
            await _unitOfWork.SaveChangesAsync();

            var respostas = await _unitOfWork.RespostasTicket.FindAsync(r => r.TicketId == ticket.Id);
            var usuarioIds = respostas.Select(r => r.UsuarioId).ToList();
            if (ticket.UsuarioAtribuidoId.HasValue)
            {
                usuarioIds.Add(ticket.UsuarioAtribuidoId.Value);
            }
            var usuarios = usuarioIds.Any()
                ? await _unitOfWork.Users.FindAsync(u => usuarioIds.Contains(u.Id))
                : Enumerable.Empty<User>();

            var clienteLookup = new Dictionary<int, Pessoa> { { cliente.Id, cliente } };
            var usuarioLookup = usuarios.ToDictionary(u => u.Id, u => u);
            var respostasLookup = new Dictionary<int, List<RespostaTicket>>
            {
                { ticket.Id, respostas.ToList() }
            };

            return TicketMapper.ToDto(ticket, clienteLookup, usuarioLookup, respostasLookup);
        }
    }
}

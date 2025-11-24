using MediatR;
using Elo.Application.DTOs.Ticket;
using Elo.Domain.Entities;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.Tickets;

public static class CreateRespostaTicket
{
    public class Command : IRequest<TicketDto>
    {
        public int TicketId { get; set; }
        public int EmpresaId { get; set; }
        public int UsuarioId { get; set; }
        public CreateRespostaTicketDto Dto { get; set; } = new();
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
            var ticket = await _unitOfWork.Tickets.GetByIdAsync(request.TicketId);
            if (ticket == null)
            {
                throw new KeyNotFoundException("Ticket não encontrado.");
            }

            var cliente = await _unitOfWork.Pessoas.GetByIdAsync(ticket.ClienteId);
            if (cliente == null || cliente.Tipo != PessoaTipo.Cliente || cliente.EmpresaId != request.EmpresaId)
            {
                throw new UnauthorizedAccessException("Ticket não pertence à empresa informada.");
            }

            var usuario = await _unitOfWork.Users.GetByIdAsync(request.UsuarioId);
            if (usuario == null)
            {
                throw new UnauthorizedAccessException("Usuário não encontrado.");
            }

            var resposta = new RespostaTicket
            {
                TicketId = ticket.Id,
                UsuarioId = request.UsuarioId,
                Mensagem = request.Dto.Mensagem,
                DataResposta = DateTime.UtcNow,
                IsInterna = request.Dto.IsInterna
            };

            await _unitOfWork.RespostasTicket.AddAsync(resposta);
            await _unitOfWork.SaveChangesAsync();

            var respostas = await _unitOfWork.RespostasTicket.FindAsync(r => r.TicketId == ticket.Id);
            var usuarioIds = respostas.Select(r => r.UsuarioId).ToList();
            if (ticket.UsuarioAtribuidoId.HasValue)
            {
                usuarioIds.Add(ticket.UsuarioAtribuidoId.Value);
            }
            usuarioIds.Add(request.UsuarioId);

            var usuarios = await _unitOfWork.Users.FindAsync(u => usuarioIds.Contains(u.Id));

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

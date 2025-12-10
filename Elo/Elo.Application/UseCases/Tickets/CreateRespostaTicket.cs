using MediatR;
using Elo.Application.DTOs.Ticket;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Tickets;

public static class CreateRespostaTicket
{
    public class Command : IRequest<RespostaTicketDto>
    {
        public int TicketId { get; set; }
        public int UsuarioId { get; set; }
        public int EmpresaId { get; set; }
        public CreateRespostaTicketDto Dto { get; set; } = new();
    }

    public class Handler : IRequestHandler<Command, RespostaTicketDto>
    {
        private readonly ITicketService _ticketService;

        public Handler(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        public async Task<RespostaTicketDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var resposta = await _ticketService.AdicionarRespostaAsync(
                request.TicketId,
                request.Dto.Mensagem,
                request.UsuarioId,
                request.EmpresaId
            );

            return new RespostaTicketDto
            {
                Id = resposta.Id,
                TicketId = resposta.TicketId,
                Mensagem = resposta.Mensagem,
                UsuarioId = resposta.UsuarioId,
                DataResposta = resposta.DataResposta
            };
        }
    }
}

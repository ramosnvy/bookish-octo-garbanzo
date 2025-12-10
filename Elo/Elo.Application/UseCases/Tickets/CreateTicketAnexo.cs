using MediatR;
using Elo.Application.DTOs.Ticket;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Tickets;

public static class CreateTicketAnexo
{
    public class Command : IRequest<TicketAnexoDto>
    {
        public int TicketId { get; set; }
        public string NomeArquivo { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public byte[] Conteudo { get; set; } = Array.Empty<byte>();
        public int UsuarioId { get; set; }
        public int EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Command, TicketAnexoDto>
    {
        private readonly ITicketService _ticketService;

        public Handler(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        public async Task<TicketAnexoDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var anexo = await _ticketService.AdicionarAnexoAsync(
                request.TicketId,
                request.NomeArquivo,
                request.ContentType,
                request.Conteudo,
                request.Conteudo.Length,
                request.UsuarioId,
                request.EmpresaId
            );

            return new TicketAnexoDto
            {
                Id = anexo.Id,
                TicketId = anexo.TicketId,
                Nome = anexo.Nome,
                MimeType = anexo.MimeType,
                Tamanho = anexo.Tamanho
            };
        }
    }
}

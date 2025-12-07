using System;
using System.Collections.Generic;
using System.Linq;
using MediatR;
using Elo.Application.DTOs.Ticket;
using Elo.Domain.Entities;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.Tickets;

public static class CreateTicketAnexo
{
    public class Command : IRequest<TicketDto>
    {
        public int TicketId { get; set; }
        public int EmpresaId { get; set; }
        public int UsuarioId { get; set; }
        public string NomeArquivo { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public byte[] Conteudo { get; set; } = Array.Empty<byte>();
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

            var anexo = new TicketAnexo
            {
                TicketId = ticket.Id,
                Nome = request.NomeArquivo,
                MimeType = request.ContentType,
                Conteudo = request.Conteudo,
                Tamanho = request.Conteudo.LongLength,
                UsuarioId = request.UsuarioId,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.TicketAnexos.AddAsync(anexo);
            await _unitOfWork.SaveChangesAsync();

            return await BuildDtoAsync(ticket);
        }

        private async Task<TicketDto> BuildDtoAsync(Ticket ticket)
        {
            var cliente = await _unitOfWork.Pessoas.GetByIdAsync(ticket.ClienteId);
            var respostas = await _unitOfWork.RespostasTicket.FindAsync(r => r.TicketId == ticket.Id);
            var anexos = await _unitOfWork.TicketAnexos.FindAsync(a => a.TicketId == ticket.Id);

            var usuarioIds = respostas.Select(r => r.UsuarioId)
                .Concat(ticket.UsuarioAtribuidoId.HasValue
                    ? new[] { ticket.UsuarioAtribuidoId.Value }
                    : Enumerable.Empty<int>())
                .Concat(anexos.Select(a => a.UsuarioId))
                .Distinct()
                .ToList();

            var usuarios = usuarioIds.Any()
                ? await _unitOfWork.Users.FindAsync(u => usuarioIds.Contains(u.Id))
                : Enumerable.Empty<User>();

            var ticketTipo = await _unitOfWork.TicketTipos.GetByIdAsync(ticket.TicketTipoId);
            var ticketTipoLookup = ticketTipo != null
                ? new Dictionary<int, TicketTipo> { { ticketTipo.Id, ticketTipo } }
                : new Dictionary<int, TicketTipo>();

            Produto? produto = null;
            if (ticket.ProdutoId.HasValue)
            {
                produto = await _unitOfWork.Produtos.GetByIdAsync(ticket.ProdutoId.Value);
            }

            Pessoa? fornecedor = null;
            if (ticket.FornecedorId.HasValue)
            {
                fornecedor = await _unitOfWork.Pessoas.GetByIdAsync(ticket.FornecedorId.Value);
            }

            var clienteLookup = cliente != null ? new Dictionary<int, Pessoa> { { cliente.Id, cliente } } : new Dictionary<int, Pessoa>();
            var usuarioLookup = usuarios.ToDictionary(u => u.Id, u => u);
            var respostasLookup = new Dictionary<int, List<RespostaTicket>>
            {
                { ticket.Id, respostas.ToList() }
            };
            var anexosLookup = new Dictionary<int, List<TicketAnexo>>
            {
                { ticket.Id, anexos.ToList() }
            };
            var produtoLookup = produto != null
                ? new Dictionary<int, Produto> { { produto.Id, produto } }
                : new Dictionary<int, Produto>();
            var fornecedorLookup = fornecedor != null
                ? new Dictionary<int, Pessoa> { { fornecedor.Id, fornecedor } }
                : new Dictionary<int, Pessoa>();

            return TicketMapper.ToDto(
                ticket,
                clienteLookup,
                usuarioLookup,
                respostasLookup,
                ticketTipoLookup,
                produtoLookup,
                fornecedorLookup,
                anexosLookup);
        }
    }
}

using System.Collections.Generic;
using System.Linq;
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

            var ticketTipo = await GetTicketTipoAsync(dto.TicketTipoId, request.EmpresaId);

            Produto? produto = null;
            if (dto.ProdutoId.HasValue)
            {
                produto = await _unitOfWork.Produtos.GetByIdAsync(dto.ProdutoId.Value);
                if (produto == null || produto.EmpresaId != request.EmpresaId)
                {
                    throw new KeyNotFoundException("Produto não encontrado para esta empresa.");
                }
            }

            Pessoa? fornecedor = null;
            if (dto.FornecedorId.HasValue)
            {
                fornecedor = await _unitOfWork.Pessoas.GetByIdAsync(dto.FornecedorId.Value);
                if (fornecedor == null || fornecedor.Tipo != PessoaTipo.Fornecedor || fornecedor.EmpresaId != request.EmpresaId)
                {
                    throw new KeyNotFoundException("Fornecedor não encontrado para esta empresa.");
                }
            }

            ticket.Titulo = dto.Titulo;
            ticket.Descricao = dto.Descricao;
            ticket.TicketTipoId = ticketTipo.Id;
            ticket.Prioridade = dto.Prioridade;
            ticket.Status = dto.Status;
            ticket.NumeroExterno = dto.NumeroExterno;
            ticket.UsuarioAtribuidoId = dto.UsuarioAtribuidoId;
            ticket.ProdutoId = dto.ProdutoId;
            ticket.FornecedorId = dto.FornecedorId;
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
            var anexos = await _unitOfWork.TicketAnexos.FindAsync(a => a.TicketId == ticket.Id);

            var usuarioIds = respostas.Select(r => r.UsuarioId).ToList();
            if (ticket.UsuarioAtribuidoId.HasValue)
            {
                usuarioIds.Add(ticket.UsuarioAtribuidoId.Value);
            }
            if (anexos.Any())
            {
                usuarioIds.AddRange(anexos.Select(a => a.UsuarioId));
            }

            var usuarios = usuarioIds.Any()
                ? await _unitOfWork.Users.FindAsync(u => usuarioIds.Contains(u.Id))
                : Enumerable.Empty<User>();

            var clienteLookup = new Dictionary<int, Pessoa> { { cliente.Id, cliente } };
            var usuarioLookup = usuarios.ToDictionary(u => u.Id, u => u);
            var ticketTipoLookup = new Dictionary<int, TicketTipo> { { ticketTipo.Id, ticketTipo } };
            var produtoLookup = produto != null
                ? new Dictionary<int, Produto> { { produto.Id, produto } }
                : new Dictionary<int, Produto>();
            var fornecedorLookup = fornecedor != null
                ? new Dictionary<int, Pessoa> { { fornecedor.Id, fornecedor } }
                : new Dictionary<int, Pessoa>();
            var respostasLookup = new Dictionary<int, List<RespostaTicket>>
            {
                { ticket.Id, respostas.ToList() }
            };
            var anexosLookup = new Dictionary<int, List<TicketAnexo>>
            {
                { ticket.Id, anexos.ToList() }
            };

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

        private async Task<TicketTipo> GetTicketTipoAsync(int tipoId, int empresaId)
        {
            var tipo = await _unitOfWork.TicketTipos.GetByIdAsync(tipoId);
            if (tipo == null || (tipo.EmpresaId.HasValue && tipo.EmpresaId != empresaId))
            {
                throw new KeyNotFoundException("Tipo de ticket não encontrado para esta empresa.");
            }

            return tipo;
        }
    }
}

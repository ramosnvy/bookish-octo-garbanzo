using System;
using System.Collections.Generic;
using System.Linq;
using MediatR;
using Elo.Application.DTOs.Ticket;
using Elo.Domain.Entities;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.Tickets;

public static class GetAllTickets
{
    public class Query : IRequest<IEnumerable<TicketDto>>
    {
        public int? EmpresaId { get; set; }
        public TicketStatus? Status { get; set; }
        public TicketPrioridade? Prioridade { get; set; }
        public int? ClienteId { get; set; }
        public int? UsuarioAtribuidoId { get; set; }
        public int? TipoId { get; set; }
        public int? ProdutoId { get; set; }
        public int? FornecedorId { get; set; }
        public DateTime? DataAberturaInicio { get; set; }
        public DateTime? DataAberturaFim { get; set; }
    }

    public class Handler : IRequestHandler<Query, IEnumerable<TicketDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<TicketDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var clientes = await _unitOfWork.Pessoas.FindAsync(p =>
                p.Tipo == PessoaTipo.Cliente &&
                (!request.EmpresaId.HasValue || p.EmpresaId == request.EmpresaId.Value));
            var clienteLookup = clientes.ToDictionary(c => c.Id, c => c);

            var tickets = (await _unitOfWork.Tickets.GetAllAsync())
                .Where(t => !request.EmpresaId.HasValue || clienteLookup.ContainsKey(t.ClienteId))
                .ToList();

            if (request.Status.HasValue)
            {
                tickets = tickets.Where(t => t.Status == request.Status.Value).ToList();
            }

            if (request.Prioridade.HasValue)
            {
                tickets = tickets.Where(t => t.Prioridade == request.Prioridade.Value).ToList();
            }

            if (request.ClienteId.HasValue)
            {
                tickets = tickets.Where(t => t.ClienteId == request.ClienteId.Value).ToList();
            }

            if (request.TipoId.HasValue)
            {
                tickets = tickets.Where(t => t.TicketTipoId == request.TipoId.Value).ToList();
            }

            if (request.ProdutoId.HasValue)
            {
                tickets = tickets.Where(t => t.ProdutoId == request.ProdutoId.Value).ToList();
            }

            if (request.FornecedorId.HasValue)
            {
                tickets = tickets.Where(t => t.FornecedorId == request.FornecedorId.Value).ToList();
            }

            if (request.UsuarioAtribuidoId.HasValue)
            {
                tickets = tickets.Where(t => t.UsuarioAtribuidoId == request.UsuarioAtribuidoId.Value).ToList();
            }

            if (request.DataAberturaInicio.HasValue)
            {
                tickets = tickets.Where(t => t.DataAbertura >= request.DataAberturaInicio.Value).ToList();
            }

            if (request.DataAberturaFim.HasValue)
            {
                tickets = tickets.Where(t => t.DataAbertura <= request.DataAberturaFim.Value).ToList();
            }

            var ticketIds = tickets.Select(t => t.Id).ToList();
            var respostas = ticketIds.Any()
                ? await _unitOfWork.RespostasTicket.FindAsync(r => ticketIds.Contains(r.TicketId))
                : Enumerable.Empty<RespostaTicket>();
            var respostasLookup = respostas
                .GroupBy(r => r.TicketId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var anexos = ticketIds.Any()
                ? await _unitOfWork.TicketAnexos.FindAsync(a => ticketIds.Contains(a.TicketId))
                : Enumerable.Empty<TicketAnexo>();
            var anexosLookup = anexos
                .GroupBy(a => a.TicketId)
                .ToDictionary(g => g.Key, g => g.ToList());

            var usuarioIds = tickets.Select(t => t.UsuarioAtribuidoId ?? 0)
                .Concat(respostas.Select(r => r.UsuarioId))
                .Concat(anexos.Select(a => a.UsuarioId))
                .Where(id => id > 0)
                .Distinct()
                .ToList();
            var usuarios = usuarioIds.Any()
                ? await _unitOfWork.Users.FindAsync(u => usuarioIds.Contains(u.Id))
                : Enumerable.Empty<User>();
            var usuarioLookup = usuarios.ToDictionary(u => u.Id, u => u);

            var produtoIds = tickets
                .Where(t => t.ProdutoId.HasValue)
                .Select(t => t.ProdutoId!.Value)
                .Distinct()
                .ToList();
            var produtos = produtoIds.Any()
                ? await _unitOfWork.Produtos.FindAsync(p => produtoIds.Contains(p.Id) && (!request.EmpresaId.HasValue || p.EmpresaId == request.EmpresaId.Value))
                : Enumerable.Empty<Produto>();
            var produtoLookup = produtos.ToDictionary(p => p.Id, p => p);

            var fornecedorIds = tickets
                .Where(t => t.FornecedorId.HasValue)
                .Select(t => t.FornecedorId!.Value)
                .Distinct()
                .ToList();
            var fornecedores = fornecedorIds.Any()
                ? await _unitOfWork.Pessoas.FindAsync(p => fornecedorIds.Contains(p.Id) && p.Tipo == PessoaTipo.Fornecedor && (!request.EmpresaId.HasValue || p.EmpresaId == request.EmpresaId.Value))
                : Enumerable.Empty<Pessoa>();
            var fornecedorLookup = fornecedores.ToDictionary(p => p.Id, p => p);

            var tipoIds = tickets.Select(t => t.TicketTipoId).Distinct().ToList();
            var ticketTipos = tipoIds.Any()
                ? await _unitOfWork.TicketTipos.FindAsync(t => tipoIds.Contains(t.Id))
                : Enumerable.Empty<TicketTipo>();
            var ticketTipoLookup = ticketTipos.ToDictionary(t => t.Id, t => t);

            return tickets
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => TicketMapper.ToDto(
                    t,
                    clienteLookup,
                    usuarioLookup,
                    respostasLookup,
                    ticketTipoLookup,
                    produtoLookup,
                    fornecedorLookup,
                    anexosLookup));
        }
    }
}

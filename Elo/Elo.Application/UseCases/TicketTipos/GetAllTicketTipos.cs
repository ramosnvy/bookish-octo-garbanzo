using System.Collections.Generic;
using System.Linq;
using MediatR;
using Elo.Application.DTOs.Ticket;
using Elo.Domain.Entities;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.TicketTipos;

public static class GetAllTicketTipos
{
    public class Query : IRequest<IEnumerable<TicketTipoDto>>
    {
        public int? EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Query, IEnumerable<TicketTipoDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<TicketTipoDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var tipos = await _unitOfWork.TicketTipos.FindAsync(t =>
                t.EmpresaId == null || (request.EmpresaId.HasValue && t.EmpresaId == request.EmpresaId.Value));

            return tipos
                .OrderBy(t => t.Ordem)
                .Select(t => new TicketTipoDto
                {
                    Id = t.Id,
                    Nome = t.Nome,
                    Descricao = t.Descricao,
                    Ordem = t.Ordem,
                    Ativo = t.Ativo,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                });
        }
    }
}

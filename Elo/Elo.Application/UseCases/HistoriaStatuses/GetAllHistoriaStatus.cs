using System.Collections.Generic;
using System.Linq;
using MediatR;
using Elo.Application.DTOs.Historia;
using Elo.Domain.Entities;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.HistoriaStatuses;

public static class GetAllHistoriaStatus
{
    public class Query : IRequest<IEnumerable<HistoriaStatusDto>>
    {
        public int? EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Query, IEnumerable<HistoriaStatusDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<HistoriaStatusDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            IEnumerable<HistoriaStatus> statuses;
            if (request.EmpresaId.HasValue)
            {
                var companyStatuses = await _unitOfWork.HistoriaStatuses.FindAsync(s => s.EmpresaId == request.EmpresaId.Value);
                if (companyStatuses.Any())
                {
                    statuses = companyStatuses;
                }
                else
                {
                    statuses = await _unitOfWork.HistoriaStatuses.FindAsync(s => s.EmpresaId == null);
                }
            }
            else
            {
                statuses = await _unitOfWork.HistoriaStatuses.FindAsync(s => s.EmpresaId == null);
            }

            return statuses
                .OrderBy(s => s.Ordem)
                .Select(s => new HistoriaStatusDto
                {
                    Id = s.Id,
                    Nome = s.Nome,
                    Descricao = s.Descricao,
                    Cor = s.Cor,
                    FechaHistoria = s.FechaHistoria,
                    Ordem = s.Ordem,
                    Ativo = s.Ativo,
                    CreatedAt = s.CreatedAt,
                    UpdatedAt = s.UpdatedAt
                });
        }
    }
}

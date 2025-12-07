using System.Collections.Generic;
using System.Linq;
using MediatR;
using Elo.Application.DTOs.Historia;
using Elo.Domain.Entities;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.HistoriaTipos;

public static class GetAllHistoriaTipos
{
    public class Query : IRequest<IEnumerable<HistoriaTipoDto>>
    {
        public int? EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Query, IEnumerable<HistoriaTipoDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<HistoriaTipoDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            IEnumerable<HistoriaTipo> tipos;
            if (request.EmpresaId.HasValue)
            {
                var companyTipos = await _unitOfWork.HistoriaTipos.FindAsync(t => t.EmpresaId == request.EmpresaId.Value);
                if (companyTipos.Any())
                {
                    tipos = companyTipos;
                }
                else
                {
                    tipos = await _unitOfWork.HistoriaTipos.FindAsync(t => t.EmpresaId == null);
                }
            }
            else
            {
                tipos = await _unitOfWork.HistoriaTipos.FindAsync(t => t.EmpresaId == null);
            }

            return tipos
                .OrderBy(t => t.Ordem)
                .Select(t => new HistoriaTipoDto
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

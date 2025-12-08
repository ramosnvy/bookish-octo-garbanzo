using MediatR;
using Elo.Application.DTOs.Empresa;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.Empresas;

public static class GetAllEmpresas
{
    public class Query : IRequest<IEnumerable<EmpresaDto>> { }

    public class Handler : IRequestHandler<Query, IEnumerable<EmpresaDto>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<EmpresaDto>> Handle(Query request, CancellationToken cancellationToken)
        {
            var empresas = await _unitOfWork.Empresas.GetAllAsync();
            return empresas.Select(e => new EmpresaDto
            {
                Id = e.Id,
                RazaoSocial = e.RazaoSocial,
                NomeFantasia = e.NomeFantasia,
                Cnpj = e.Cnpj,
                Ie = e.Ie,
                Email = e.Email,
                Telefone = e.Telefone,
                Endereco = e.Endereco,
                Ativo = e.Ativo,
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt
            });
        }
    }
}

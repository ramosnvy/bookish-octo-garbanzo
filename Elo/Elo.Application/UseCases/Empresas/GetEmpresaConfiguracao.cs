using MediatR;
using Elo.Application.DTOs.Empresa;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.Empresas;

public static class GetEmpresaConfiguracao
{
    public class Query : IRequest<EmpresaConfiguracaoDto?>
    {
        public int EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Query, EmpresaConfiguracaoDto?>
    {
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EmpresaConfiguracaoDto?> Handle(Query request, CancellationToken cancellationToken)
        {
            var configs = await _unitOfWork.EmpresaConfiguracoes.FindAsync(c => c.EmpresaId == request.EmpresaId);
            var config = configs.FirstOrDefault();

            if (config == null)
            {
                return new EmpresaConfiguracaoDto
                {
                    EmpresaId = request.EmpresaId,
                    JurosValor = 0,
                    JurosTipo = Domain.Enums.TipoValor.Porcentagem, // Assuming this enum exists and is compatible
                    MoraValor = 0,
                    MoraTipo = Domain.Enums.TipoValor.Porcentagem,
                    DiaPagamentoAfiliado = 1
                };
            }

            return new EmpresaConfiguracaoDto
            {
                Id = config.Id,
                EmpresaId = config.EmpresaId,
                JurosValor = config.JurosValor,
                JurosTipo = config.JurosTipo,
                MoraValor = config.MoraValor,
                MoraTipo = config.MoraTipo,
                DiaPagamentoAfiliado = config.DiaPagamentoAfiliado
            };
        }
    }
}

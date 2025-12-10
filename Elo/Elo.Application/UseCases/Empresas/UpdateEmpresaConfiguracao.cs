using MediatR;
using Elo.Application.DTOs.Empresa;
using Elo.Domain.Entities;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.Empresas;

public static class UpdateEmpresaConfiguracao
{
    public class Command : IRequest<EmpresaConfiguracaoDto>
    {
        public int EmpresaId { get; set; }
        public UpdateEmpresaConfiguracaoDto Dto { get; set; } = new();
    }

    public class Handler : IRequestHandler<Command, EmpresaConfiguracaoDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<EmpresaConfiguracaoDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var configs = await _unitOfWork.EmpresaConfiguracoes.FindAsync(c => c.EmpresaId == request.EmpresaId);
            var config = configs.FirstOrDefault();

            if (config == null)
            {
                config = new EmpresaConfiguracao
                {
                    EmpresaId = request.EmpresaId
                };
                await _unitOfWork.EmpresaConfiguracoes.AddAsync(config);
            }

            config.JurosValor = request.Dto.JurosValor;
            config.JurosTipo = request.Dto.JurosTipo;
            config.MoraValor = request.Dto.MoraValor;
            config.MoraTipo = request.Dto.MoraTipo;
            config.DiaPagamentoAfiliado = request.Dto.DiaPagamentoAfiliado;

            // Normalize DiaPagamentoAfiliado
            if (config.DiaPagamentoAfiliado < 1) config.DiaPagamentoAfiliado = 1;
            if (config.DiaPagamentoAfiliado > 31) config.DiaPagamentoAfiliado = 31;

            if (config.Id > 0)
            {
                await _unitOfWork.EmpresaConfiguracoes.UpdateAsync(config);
            }
            
            await _unitOfWork.SaveChangesAsync();

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

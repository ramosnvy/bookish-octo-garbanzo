using MediatR;
using Elo.Application.DTOs.Empresa;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Empresas;

public static class UpdateEmpresaStatus
{
    public class Command : IRequest<EmpresaDto>
    {
        public int Id { get; set; }
        public bool Ativo { get; set; }
    }

    public class Handler : IRequestHandler<Command, EmpresaDto>
    {
        private readonly IEmpresaService _empresaService;

        public Handler(IEmpresaService empresaService)
        {
            _empresaService = empresaService;
        }

        public async Task<EmpresaDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var atualizada = await _empresaService.AtualizarStatusAsync(request.Id, request.Ativo);

            return new EmpresaDto
            {
                Id = atualizada.Id,
                RazaoSocial = atualizada.RazaoSocial,
                NomeFantasia = atualizada.NomeFantasia,
                Cnpj = atualizada.Cnpj,
                Ie = atualizada.Ie,
                Email = atualizada.Email,
                Telefone = atualizada.Telefone,
                Endereco = atualizada.Endereco,
                Ativo = atualizada.Ativo,
                CreatedAt = atualizada.CreatedAt,
                UpdatedAt = atualizada.UpdatedAt
            };
        }
    }
}

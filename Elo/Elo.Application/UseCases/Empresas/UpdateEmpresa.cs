using MediatR;
using Elo.Application.DTOs.Empresa;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Empresas;

public static class UpdateEmpresa
{
    public class Command : IRequest<EmpresaDto>
    {
        public int Id { get; set; }
        public string RazaoSocial { get; set; } = string.Empty;
        public string NomeFantasia { get; set; } = string.Empty;
        public string Cnpj { get; set; } = string.Empty;
        public string Ie { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Endereco { get; set; } = string.Empty;
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
            // Service handles logic
            var atualizada = await _empresaService.AtualizarEmpresaAsync(
                request.Id,
                request.RazaoSocial,
                request.NomeFantasia,
                request.Cnpj,
                request.Ie,
                request.Email,
                request.Telefone,
                request.Endereco,
                request.Ativo
            );

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

using System.Collections.Generic;
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
            var empresa = await _empresaService.ObterPorIdAsync(request.Id);
            if (empresa == null)
            {
                throw new KeyNotFoundException($"Empresa com ID {request.Id} não encontrada");
            }

            empresa.RazaoSocial = request.RazaoSocial;
            empresa.NomeFantasia = request.NomeFantasia;
            empresa.Cnpj = request.Cnpj;
            empresa.Ie = request.Ie;
            empresa.Email = request.Email;
            empresa.Telefone = request.Telefone;
            empresa.Endereco = request.Endereco;
            empresa.Ativo = request.Ativo;
            empresa.UpdatedAt = DateTime.UtcNow;

            var atualizada = await _empresaService.AtualizarEmpresaAsync(empresa);

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

using MediatR;
using Elo.Application.DTOs.Empresa;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Empresas;

public static class CreateEmpresa
{
    public class Command : IRequest<EmpresaDto>
    {
        public string RazaoSocial { get; set; } = string.Empty;
        public string NomeFantasia { get; set; } = string.Empty;
        public string Cnpj { get; set; } = string.Empty;
        public string Ie { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Endereco { get; set; } = string.Empty;
        public bool Ativo { get; set; } = true;
    }

    public class Handler : IRequestHandler<Command, EmpresaDto>
    {
        private readonly IEmpresaService _empresaService;
        private readonly ITicketTipoService _ticketTipoService;
        private readonly IHistoriaTipoService _historiaTipoService;
        private readonly IHistoriaStatusService _historiaStatusService;

        public Handler(
            IEmpresaService empresaService,
            ITicketTipoService ticketTipoService,
            IHistoriaTipoService historiaTipoService,
            IHistoriaStatusService historiaStatusService)
        {
            _empresaService = empresaService;
            _ticketTipoService = ticketTipoService;
            _historiaTipoService = historiaTipoService;
            _historiaStatusService = historiaStatusService;
        }

        public async Task<EmpresaDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var criada = await _empresaService.CriarEmpresaAsync(
                request.RazaoSocial,
                request.NomeFantasia,
                request.Cnpj,
                request.Ie,
                request.Email,
                request.Telefone,
                request.Endereco,
                request.Ativo
            );

            // Create default data for the new company
            await CreateDefaultDataAsync(criada.Id);

            return new EmpresaDto
            {
                Id = criada.Id,
                RazaoSocial = criada.RazaoSocial,
                NomeFantasia = criada.NomeFantasia,
                Cnpj = criada.Cnpj,
                Ie = criada.Ie,
                Email = criada.Email,
                Telefone = criada.Telefone,
                Endereco = criada.Endereco,
                Ativo = criada.Ativo,
                CreatedAt = criada.CreatedAt,
                UpdatedAt = criada.UpdatedAt
            };
        }

        private async Task CreateDefaultDataAsync(int empresaId)
        {
            // Default Ticket Types
            await _ticketTipoService.CriarAsync("Suporte", "Suporte técnico geral", 1, true, empresaId);
            await _ticketTipoService.CriarAsync("Bug", "Relato de erro no sistema", 2, true, empresaId);
            await _ticketTipoService.CriarAsync("Melhoria", "Sugestão de melhoria", 3, true, empresaId);

            // Default Implantation Types (HistoriaTipos)
            await _historiaTipoService.CriarAsync("Implantação Padrão", "Processo padrão de implantação", 1, true, empresaId);
            await _historiaTipoService.CriarAsync("Treinamento", "Sessão de treinamento", 2, true, empresaId);

            // Default Implantation Status (HistoriaStatus)
            await _historiaStatusService.CriarAsync("Aberto", "#64748b", 1, false, true, empresaId); // Slate 500
            await _historiaStatusService.CriarAsync("Em Andamento", "#3b82f6", 2, false, true, empresaId); // Blue 500
            await _historiaStatusService.CriarAsync("Pendente Cliente", "#f97316", 3, false, true, empresaId); // Orange 500
            await _historiaStatusService.CriarAsync("Concluído", "#22c55e", 4, true, true, empresaId); // Green 500
        }
    }
}

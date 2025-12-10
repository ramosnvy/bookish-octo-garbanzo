using System.Security.Claims;
using Elo.Domain.Interfaces;
using Elo.Domain.Interfaces.Repositories;
using Microsoft.AspNetCore.Http;

namespace Elo.Infrastructure.Services;

public class EmpresaContextService : IEmpresaContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;

    public EmpresaContextService(IHttpContextAccessor httpContextAccessor, IUnitOfWork unitOfWork)
    {
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
    }

    public bool IsGlobalAdmin =>
        string.Equals(GetUser()?.FindFirst(ClaimTypes.Role)?.Value, "Admin", StringComparison.OrdinalIgnoreCase)
        && !UsuarioEmpresaId.HasValue;

    public int? UsuarioEmpresaId => GetCompanyId();

    public async Task<int?> ResolveEmpresaAsync(int? requestedEmpresaId = null, CancellationToken cancellationToken = default)
    {
        if (IsGlobalAdmin)
        {
            if (!requestedEmpresaId.HasValue)
            {
                return null;
            }

            var empresa = await _unitOfWork.Empresas.GetByIdAsync(requestedEmpresaId.Value);
            if (empresa == null)
            {
                throw new KeyNotFoundException("Empresa informada não foi encontrada.");
            }

            return empresa.Id;
        }

        var empresaId = UsuarioEmpresaId;
        if (!empresaId.HasValue)
        {
            throw new UnauthorizedAccessException("Usuário não está associado a uma empresa.");
        }

        return empresaId.Value;
    }

    public async Task<int> RequireEmpresaAsync(int? requestedEmpresaId = null, CancellationToken cancellationToken = default)
    {
        var resolved = await ResolveEmpresaAsync(requestedEmpresaId, cancellationToken);
        if (!resolved.HasValue)
        {
            throw new InvalidOperationException("Selecione uma empresa antes de executar esta operação.");
        }

        return resolved.Value;
    }

    private ClaimsPrincipal? GetUser()
    {
        return _httpContextAccessor.HttpContext?.User;
    }

    private int? GetCompanyId()
    {
        var claim = GetUser()?.FindFirst("companyId");
        return claim != null && int.TryParse(claim.Value, out var empresaId) ? empresaId : null;
    }
}

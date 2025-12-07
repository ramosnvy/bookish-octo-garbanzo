using System.Threading;
using System.Threading.Tasks;

namespace Elo.Application.Interfaces;

public interface IEmpresaContextService
{
    bool IsGlobalAdmin { get; }
    int? UsuarioEmpresaId { get; }

    Task<int?> ResolveEmpresaAsync(int? requestedEmpresaId = null, CancellationToken cancellationToken = default);
    Task<int> RequireEmpresaAsync(int? requestedEmpresaId = null, CancellationToken cancellationToken = default);
}

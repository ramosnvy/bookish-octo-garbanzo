using Elo.Domain.Entities;

namespace Elo.Domain.Interfaces;

public record AssinaturaItemInput(int ProdutoId, int? ProdutoModuloId);

public interface IAssinaturaService
{
    Task<Assinatura> CriarAssinaturaAsync(
        int empresaId,
        int clienteId,
        bool isRecorrente,
        int? intervaloDias,
        int? recorrenciaQtde,
        DateTime dataInicio,
        DateTime? dataFim,
        bool gerarFinanceiro,
        bool gerarImplantacao,
        List<AssinaturaItemInput> itens,
        Elo.Domain.Enums.FormaPagamento? formaPagamento,
        int? afiliadoId = null,
        int? usuarioCriadorId = null);

    Task<IEnumerable<Assinatura>> ObterAssinaturasAsync(int empresaId);
    Task<IEnumerable<AssinaturaItem>> ObterItensPorAssinaturaIdsAsync(IEnumerable<int> assinaturaIds);
    Task CancelarAssinaturaAsync(int assinaturaId, int empresaId);
}

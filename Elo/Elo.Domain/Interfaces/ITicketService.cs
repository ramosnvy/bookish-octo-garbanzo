using Elo.Domain.Entities;
using Elo.Domain.Enums;

namespace Elo.Domain.Interfaces;

public interface ITicketService
{
    Task<Ticket> CriarTicketAsync(string titulo, string descricao, int clienteId, int ticketTipoId, int? produtoId, int? fornecedorId, int? usuarioAtribuidoId, int empresaId, string? numeroExterno = null);
    Task<Ticket> AtualizarTicketAsync(int id, string titulo, string descricao, int ticketTipoId, int? produtoId, int? fornecedorId, int? usuarioAtribuidoId, TicketStatus status, int empresaId);
    Task<bool> DeletarTicketAsync(int id, int empresaId);
    Task<Ticket?> ObterTicketPorIdAsync(int id, int empresaId);
    Task<IEnumerable<Ticket>> ObterTicketsAsync(int? empresaId = null, int? clienteId = null, TicketStatus? status = null);
    Task<TicketAnexo> AdicionarAnexoAsync(int ticketId, string nome, string mimeType, byte[] conteudo, long tamanho, int usuarioId, int empresaId);
    Task<RespostaTicket> AdicionarRespostaAsync(int ticketId, string mensagem, int usuarioId, int empresaId);
}

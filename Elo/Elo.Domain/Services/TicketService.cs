using Elo.Domain.Entities;
using Elo.Domain.Enums;
using Elo.Domain.Exceptions;
using Elo.Domain.Interfaces;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Domain.Services;

public class TicketService : ITicketService
{
    private readonly IUnitOfWork _unitOfWork;

    public TicketService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Ticket> CriarTicketAsync(string titulo, string descricao, int clienteId, int ticketTipoId, int? produtoId, int? fornecedorId, int? usuarioAtribuidoId, int empresaId, string? numeroExterno = null)
    {
        // Validar cliente
        var cliente = await _unitOfWork.Pessoas.GetByIdAsync(clienteId);
        if (cliente == null || cliente.EmpresaId != empresaId)
            throw new KeyNotFoundException("Cliente não encontrado para esta empresa");
        
        if (cliente.Tipo != PessoaTipo.Cliente)
            throw new InvalidOperationException("A pessoa informada não é um cliente.");
        
        if (cliente.Status != Status.Ativo)
            throw new PessoaInativaException(clienteId, PessoaTipo.Cliente);

        // Validar produto (se informado)
        if (produtoId.HasValue)
        {
            var produto = await _unitOfWork.Produtos.GetByIdAsync(produtoId.Value);
            if (produto == null || produto.EmpresaId != empresaId)
                throw new KeyNotFoundException("Produto não encontrado.");
            
            if (!produto.Ativo)
                throw new ProdutoInativoException(produtoId.Value);
        }

        // Validar fornecedor (se informado)
        if (fornecedorId.HasValue)
        {
            var fornecedor = await _unitOfWork.Pessoas.GetByIdAsync(fornecedorId.Value);
            if (fornecedor == null || fornecedor.EmpresaId != empresaId)
                throw new KeyNotFoundException("Fornecedor não encontrado.");
            
            if (fornecedor.Tipo != PessoaTipo.Fornecedor)
                throw new InvalidOperationException("A pessoa informada não é um fornecedor.");
            
            if (fornecedor.Status != Status.Ativo)
                throw new PessoaInativaException(fornecedorId.Value, PessoaTipo.Fornecedor);
        }

        // Validar usuário atribuído (se informado)
        if (usuarioAtribuidoId.HasValue)
        {
            var usuarioAtribuido = await _unitOfWork.Users.GetByIdAsync(usuarioAtribuidoId.Value);
            if (usuarioAtribuido == null)
                throw new KeyNotFoundException("Usuário atribuído não encontrado.");
            
            if (usuarioAtribuido.Status != Status.Ativo)
                throw new UsuarioInativoException(usuarioAtribuidoId.Value);
        }

        var ticket = new Ticket
        {
            Titulo = titulo,
            Descricao = descricao,
            ClienteId = clienteId,
            EmpresaId = empresaId,
            TicketTipoId = ticketTipoId,
            ProdutoId = produtoId,
            FornecedorId = fornecedorId,
            UsuarioAtribuidoId = usuarioAtribuidoId,
            Status = TicketStatus.Aberto,
            NumeroExterno = numeroExterno ?? string.Empty,
            DataAbertura = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _unitOfWork.Tickets.AddAsync(ticket);
        await _unitOfWork.SaveChangesAsync();

        return created;
    }

    public async Task<Ticket> AtualizarTicketAsync(int id, string titulo, string descricao, int ticketTipoId, int? produtoId, int? fornecedorId, int? usuarioAtribuidoId, TicketStatus status, int empresaId)
    {
        var ticket = await _unitOfWork.Tickets.GetByIdAsync(id);
        if (ticket == null)
            throw new TicketNaoEncontradoException(id);

        // Validar através do cliente
        var cliente = await _unitOfWork.Pessoas.GetByIdAsync(ticket.ClienteId);
        if (cliente == null || cliente.EmpresaId != empresaId)
            throw new UnauthorizedAccessException("Ticket não pertence à empresa informada");

        ticket.Titulo = titulo;
        ticket.Descricao = descricao;
        ticket.TicketTipoId = ticketTipoId;
        ticket.ProdutoId = produtoId;
        ticket.FornecedorId = fornecedorId;
        ticket.UsuarioAtribuidoId = usuarioAtribuidoId;
        ticket.Status = status;
        ticket.UpdatedAt = DateTime.UtcNow;

        if (status == TicketStatus.Resolvido || status == TicketStatus.Fechado || status == TicketStatus.Cancelado)
        {
            ticket.DataFechamento = DateTime.UtcNow;
        }

        await _unitOfWork.Tickets.UpdateAsync(ticket);
        await _unitOfWork.SaveChangesAsync();

        return ticket;
    }

    public async Task<bool> DeletarTicketAsync(int id, int empresaId)
    {
        var ticket = await _unitOfWork.Tickets.GetByIdAsync(id);
        if (ticket == null)
            throw new TicketNaoEncontradoException(id);

        // Validar através do cliente
        var cliente = await _unitOfWork.Pessoas.GetByIdAsync(ticket.ClienteId);
        if (cliente == null || cliente.EmpresaId != empresaId)
            throw new UnauthorizedAccessException("Ticket não pertence à empresa informada");

        await _unitOfWork.Tickets.DeleteAsync(ticket);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<Ticket?> ObterTicketPorIdAsync(int id, int empresaId)
    {
        var ticket = await _unitOfWork.Tickets.GetByIdAsync(id);
        
        if (ticket == null)
            return null;

        // Validar através do cliente
        var cliente = await _unitOfWork.Pessoas.GetByIdAsync(ticket.ClienteId);
        if (cliente == null || cliente.EmpresaId != empresaId)
            throw new UnauthorizedAccessException("Ticket não pertence à empresa informada");

        return ticket;
    }

    public async Task<IEnumerable<Ticket>> ObterTicketsAsync(int? empresaId = null, int? clienteId = null, TicketStatus? status = null)
    {
        var tickets = await _unitOfWork.Tickets.GetAllAsync();

        if (empresaId.HasValue)
        {
            tickets = tickets.Where(t => t.EmpresaId == empresaId.Value);
        }

        if (clienteId.HasValue)
            tickets = tickets.Where(t => t.ClienteId == clienteId.Value);

        if (status.HasValue)
            tickets = tickets.Where(t => t.Status == status.Value);

        return tickets;
    }

    public async Task<TicketAnexo> AdicionarAnexoAsync(int ticketId, string nome, string mimeType, byte[] conteudo, long tamanho, int usuarioId, int empresaId)
    {
        var ticket = await _unitOfWork.Tickets.GetByIdAsync(ticketId);
        if (ticket == null)
            throw new TicketNaoEncontradoException(ticketId);

        // Validar através do cliente
        var cliente = await _unitOfWork.Pessoas.GetByIdAsync(ticket.ClienteId);
        if (cliente == null || cliente.EmpresaId != empresaId)
            throw new UnauthorizedAccessException("Ticket não pertence à empresa informada");

        var anexo = new TicketAnexo
        {
            TicketId = ticketId,
            Nome = nome,
            MimeType = mimeType,
            Conteudo = conteudo,
            Tamanho = tamanho,
            UsuarioId = usuarioId,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _unitOfWork.TicketAnexos.AddAsync(anexo);
        await _unitOfWork.SaveChangesAsync();

        return created;
    }

    public async Task<RespostaTicket> AdicionarRespostaAsync(int ticketId, string mensagem, int usuarioId, int empresaId)
    {
        var ticket = await _unitOfWork.Tickets.GetByIdAsync(ticketId);
        if (ticket == null)
            throw new TicketNaoEncontradoException(ticketId);

        // Validar através do cliente
        var cliente = await _unitOfWork.Pessoas.GetByIdAsync(ticket.ClienteId);
        if (cliente == null || cliente.EmpresaId != empresaId)
            throw new UnauthorizedAccessException("Ticket não pertence à empresa informada");

        var resposta = new RespostaTicket
        {
            TicketId = ticketId,
            Mensagem = mensagem,
            UsuarioId = usuarioId,
            EmpresaId = empresaId,
            DataResposta = DateTime.UtcNow
        };

        var created = await _unitOfWork.RespostasTicket.AddAsync(resposta);
        await _unitOfWork.SaveChangesAsync();

        return created;
    }
}

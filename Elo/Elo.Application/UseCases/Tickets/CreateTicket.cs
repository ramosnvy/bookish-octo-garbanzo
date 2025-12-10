using MediatR;
using Elo.Application.DTOs.Ticket;
using Elo.Domain.Interfaces;
using Elo.Domain.Entities;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.Tickets;

public static class CreateTicket
{
    public class Command : IRequest<TicketDto>
    {
        public int EmpresaId { get; set; }
        public CreateTicketDto Dto { get; set; } = new();
    }

    public class Handler : IRequestHandler<Command, TicketDto>
    {
        private readonly ITicketService _ticketService;
        private readonly IUnitOfWork _unitOfWork;

        public Handler(ITicketService ticketService, IUnitOfWork unitOfWork)
        {
            _ticketService = ticketService;
            _unitOfWork = unitOfWork;
        }

        public async Task<TicketDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;
            
            // Validações de referências (mantidas aqui pois são validações de integridade)
            await ValidarReferenciasAsync(dto, request.EmpresaId);

            // Delegar criação ao service
            var ticket = await _ticketService.CriarTicketAsync(
                dto.Titulo,
                dto.Descricao,
                dto.ClienteId,
                dto.TicketTipoId,
                dto.ProdutoId,
                dto.FornecedorId,
                dto.UsuarioAtribuidoId,
                request.EmpresaId,
                dto.NumeroExterno
            );

            // Carregar dados para o DTO
            return await MapearParaDtoAsync(ticket);
        }

        private async Task ValidarReferenciasAsync(CreateTicketDto dto, int empresaId)
        {
            var cliente = await _unitOfWork.Pessoas.GetByIdAsync(dto.ClienteId);
            if (cliente == null || cliente.Tipo != PessoaTipo.Cliente || cliente.EmpresaId != empresaId)
                throw new KeyNotFoundException("Cliente não encontrado para esta empresa.");

            if (dto.UsuarioAtribuidoId.HasValue)
            {
                var usuario = await _unitOfWork.Users.GetByIdAsync(dto.UsuarioAtribuidoId.Value);
                if (usuario == null || usuario.EmpresaId != empresaId)
                    throw new KeyNotFoundException("Usuário atribuído não encontrado para esta empresa.");
            }

            var ticketTipo = await _unitOfWork.TicketTipos.GetByIdAsync(dto.TicketTipoId);
            if (ticketTipo == null || (ticketTipo.EmpresaId.HasValue && ticketTipo.EmpresaId != empresaId))
                throw new KeyNotFoundException("Tipo de ticket não encontrado para esta empresa.");

            if (dto.ProdutoId.HasValue)
            {
                var produto = await _unitOfWork.Produtos.GetByIdAsync(dto.ProdutoId.Value);
                if (produto == null || produto.EmpresaId != empresaId)
                    throw new KeyNotFoundException("Produto não encontrado para esta empresa.");
            }

            if (dto.FornecedorId.HasValue)
            {
                var fornecedor = await _unitOfWork.Pessoas.GetByIdAsync(dto.FornecedorId.Value);
                if (fornecedor == null || fornecedor.Tipo != PessoaTipo.Fornecedor || fornecedor.EmpresaId != empresaId)
                    throw new KeyNotFoundException("Fornecedor não encontrado para esta empresa.");
            }
        }

        private async Task<TicketDto> MapearParaDtoAsync(Ticket ticket)
        {
            var cliente = await _unitOfWork.Pessoas.GetByIdAsync(ticket.ClienteId);
            var ticketTipo = await _unitOfWork.TicketTipos.GetByIdAsync(ticket.TicketTipoId);

            User? usuario = null;
            if (ticket.UsuarioAtribuidoId.HasValue)
                usuario = await _unitOfWork.Users.GetByIdAsync(ticket.UsuarioAtribuidoId.Value);

            Produto? produto = null;
            if (ticket.ProdutoId.HasValue)
                produto = await _unitOfWork.Produtos.GetByIdAsync(ticket.ProdutoId.Value);

            Pessoa? fornecedor = null;
            if (ticket.FornecedorId.HasValue)
                fornecedor = await _unitOfWork.Pessoas.GetByIdAsync(ticket.FornecedorId.Value);

            var clienteLookup = new Dictionary<int, Pessoa> { { cliente!.Id, cliente } };
            var usuarioLookup = usuario != null ? new Dictionary<int, User> { { usuario.Id, usuario } } : new Dictionary<int, User>();
            var ticketTipoLookup = new Dictionary<int, TicketTipo> { { ticketTipo!.Id, ticketTipo } };
            var produtoLookup = produto != null ? new Dictionary<int, Produto> { { produto.Id, produto } } : new Dictionary<int, Produto>();
            var fornecedorLookup = fornecedor != null ? new Dictionary<int, Pessoa> { { fornecedor.Id, fornecedor } } : new Dictionary<int, Pessoa>();
            var respostasLookup = new Dictionary<int, List<RespostaTicket>> { { ticket.Id, new List<RespostaTicket>() } };
            var anexosLookup = new Dictionary<int, List<TicketAnexo>> { { ticket.Id, new List<TicketAnexo>() } };

            return TicketMapper.ToDto(ticket, clienteLookup, usuarioLookup, respostasLookup, ticketTipoLookup, produtoLookup, fornecedorLookup, anexosLookup);
        }
    }
}

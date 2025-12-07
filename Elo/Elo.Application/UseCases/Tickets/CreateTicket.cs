using MediatR;
using Elo.Application.DTOs.Ticket;
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
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<TicketDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;
            var cliente = await _unitOfWork.Pessoas.GetByIdAsync(dto.ClienteId);
            if (cliente == null || cliente.Tipo != PessoaTipo.Cliente || cliente.EmpresaId != request.EmpresaId)
            {
                throw new KeyNotFoundException("Cliente não encontrado para esta empresa.");
            }

            User? usuarioAtribuido = null;
            if (dto.UsuarioAtribuidoId.HasValue)
            {
                usuarioAtribuido = await _unitOfWork.Users.GetByIdAsync(dto.UsuarioAtribuidoId.Value);
                if (usuarioAtribuido == null || usuarioAtribuido.EmpresaId != request.EmpresaId)
                {
                    throw new KeyNotFoundException("Usuário atribuído não encontrado para esta empresa.");
                }
            }

            var ticketTipo = await GetTicketTipoAsync(dto.TicketTipoId, request.EmpresaId);

            Produto? produto = null;
            if (dto.ProdutoId.HasValue)
            {
                produto = await _unitOfWork.Produtos.GetByIdAsync(dto.ProdutoId.Value);
                if (produto == null || produto.EmpresaId != request.EmpresaId)
                {
                    throw new KeyNotFoundException("Produto não encontrado para esta empresa.");
                }
            }

            Pessoa? fornecedor = null;
            if (dto.FornecedorId.HasValue)
            {
                fornecedor = await _unitOfWork.Pessoas.GetByIdAsync(dto.FornecedorId.Value);
                if (fornecedor == null || fornecedor.Tipo != PessoaTipo.Fornecedor || fornecedor.EmpresaId != request.EmpresaId)
                {
                    throw new KeyNotFoundException("Fornecedor não encontrado para esta empresa.");
                }
            }

            var ticket = new Ticket
            {
                ClienteId = dto.ClienteId,
                Titulo = dto.Titulo,
                Descricao = dto.Descricao,
                TicketTipoId = ticketTipo.Id,
                Prioridade = dto.Prioridade,
                Status = dto.Status,
                NumeroExterno = dto.NumeroExterno,
                UsuarioAtribuidoId = dto.UsuarioAtribuidoId,
                ProdutoId = dto.ProdutoId,
                FornecedorId = dto.FornecedorId,
                DataAbertura = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            if (dto.Status == TicketStatus.Resolvido || dto.Status == TicketStatus.Fechado || dto.Status == TicketStatus.Cancelado)
            {
                ticket.DataFechamento = DateTime.UtcNow;
            }

            await _unitOfWork.Tickets.AddAsync(ticket);
            await _unitOfWork.SaveChangesAsync();

            var clienteLookup = new Dictionary<int, Pessoa> { { cliente.Id, cliente } };
            var usuarioLookup = usuarioAtribuido != null
                ? new Dictionary<int, User> { { usuarioAtribuido.Id, usuarioAtribuido } }
                : new Dictionary<int, User>();
            var ticketTipoLookup = new Dictionary<int, TicketTipo> { { ticketTipo.Id, ticketTipo } };
            var produtoLookup = produto != null
                ? new Dictionary<int, Produto> { { produto.Id, produto } }
                : new Dictionary<int, Produto>();
            var fornecedorLookup = fornecedor != null
                ? new Dictionary<int, Pessoa> { { fornecedor.Id, fornecedor } }
                : new Dictionary<int, Pessoa>();
            var respostasLookup = new Dictionary<int, List<RespostaTicket>>
            {
                { ticket.Id, new List<RespostaTicket>() }
            };
            var anexosLookup = new Dictionary<int, List<TicketAnexo>>
            {
                { ticket.Id, new List<TicketAnexo>() }
            };

            return TicketMapper.ToDto(
                ticket,
                clienteLookup,
                usuarioLookup,
                respostasLookup,
                ticketTipoLookup,
                produtoLookup,
                fornecedorLookup,
                anexosLookup);
        }

        private async Task<TicketTipo> GetTicketTipoAsync(int tipoId, int empresaId)
        {
            var tipo = await _unitOfWork.TicketTipos.GetByIdAsync(tipoId);
            if (tipo == null || (tipo.EmpresaId.HasValue && tipo.EmpresaId != empresaId))
            {
                throw new KeyNotFoundException("Tipo de ticket não encontrado para esta empresa.");
            }

            return tipo;
        }
    }
}

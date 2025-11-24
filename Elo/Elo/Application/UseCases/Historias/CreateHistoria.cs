using MediatR;
using Elo.Application.DTOs.Historia;
using Elo.Domain.Entities;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.Historias;

public static class CreateHistoria
{
    public class Command : IRequest<HistoriaDto>
    {
        public int EmpresaId { get; set; }
        public CreateHistoriaDto Dto { get; set; } = new();
        public int RequesterUserId { get; set; }
        public bool IsGlobalAdmin { get; set; }
    }

    public class Handler : IRequestHandler<Command, HistoriaDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<HistoriaDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;
            var cliente = await _unitOfWork.Pessoas.GetByIdAsync(dto.ClienteId);
            if (cliente == null || cliente.Tipo != PessoaTipo.Cliente || cliente.EmpresaId != request.EmpresaId)
            {
                throw new KeyNotFoundException("Cliente não encontrado para esta empresa.");
            }

            var produto = await _unitOfWork.Produtos.GetByIdAsync(dto.ProdutoId);
            if (produto == null || produto.EmpresaId != request.EmpresaId)
            {
                throw new KeyNotFoundException("Produto não encontrado para esta empresa.");
            }

            var responsavel = await _unitOfWork.Users.GetByIdAsync(dto.UsuarioResponsavelId);
            if (responsavel == null || (!request.IsGlobalAdmin && responsavel.EmpresaId != request.EmpresaId))
            {
                throw new KeyNotFoundException("Usuário responsável não encontrado para esta empresa.");
            }

            var status = dto.Status;

            var historia = new Historia
            {
                ClienteId = dto.ClienteId,
                ProdutoId = dto.ProdutoId,
                Status = status,
                Tipo = dto.Tipo,
                UsuarioResponsavelId = dto.UsuarioResponsavelId,
                DataInicio = dto.DataInicio ?? DateTime.UtcNow,
                DataFinalizacao = dto.DataFinalizacao,
                Observacoes = dto.Observacoes,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.Historias.AddAsync(historia);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.HistoriaMovimentacoes.AddAsync(new HistoriaMovimentacao
            {
                HistoriaId = historia.Id,
                StatusAnterior = status,
                StatusNovo = status,
                UsuarioId = request.RequesterUserId,
                DataMovimentacao = DateTime.UtcNow,
                Observacoes = "História criada."
            });
            await _unitOfWork.SaveChangesAsync();

            return await BuildDtoAsync(historia);
        }

        private async Task<HistoriaDto> BuildDtoAsync(Historia historia)
        {
            var clientes = await _unitOfWork.Pessoas.FindAsync(p => p.Id == historia.ClienteId);
            var produtos = await _unitOfWork.Produtos.FindAsync(p => p.Id == historia.ProdutoId);
            var responsaveis = await _unitOfWork.Users.FindAsync(u => u.Id == historia.UsuarioResponsavelId);
            var movimentos = await _unitOfWork.HistoriaMovimentacoes.FindAsync(m => m.HistoriaId == historia.Id);

            var clienteLookup = clientes.ToDictionary(c => c.Id, c => c);
            var produtoLookup = produtos.ToDictionary(p => p.Id, p => p);
            var usuarioIds = movimentos.Select(m => m.UsuarioId).Append(historia.UsuarioResponsavelId).Distinct().ToList();
            var usuarios = await _unitOfWork.Users.FindAsync(u => usuarioIds.Contains(u.Id));
            var usuarioLookup = usuarios.ToDictionary(u => u.Id, u => u);
            var movimentosLookup = new Dictionary<int, List<HistoriaMovimentacao>>
            {
                { historia.Id, movimentos.ToList() }
            };

            return HistoriaMapper.ToDto(historia, clienteLookup, produtoLookup, usuarioLookup, movimentosLookup);
        }
    }
}

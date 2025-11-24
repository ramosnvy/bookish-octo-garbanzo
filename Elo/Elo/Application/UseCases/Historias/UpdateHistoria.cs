using MediatR;
using Elo.Application.DTOs.Historia;
using Elo.Domain.Entities;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.Historias;

public static class UpdateHistoria
{
    public class Command : IRequest<HistoriaDto>
    {
        public int EmpresaId { get; set; }
        public UpdateHistoriaDto Dto { get; set; } = new();
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
            var historia = await _unitOfWork.Historias.GetByIdAsync(dto.Id);
            if (historia == null)
            {
                throw new KeyNotFoundException("Ação não encontrada.");
            }

            var cliente = await _unitOfWork.Pessoas.GetByIdAsync(historia.ClienteId);
            if (cliente == null || cliente.Tipo != PessoaTipo.Cliente || cliente.EmpresaId != request.EmpresaId)
            {
                throw new UnauthorizedAccessException("História não pertence à empresa informada.");
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

            var statusAnterior = historia.Status;
            historia.ProdutoId = dto.ProdutoId;
            historia.UsuarioResponsavelId = dto.UsuarioResponsavelId;
            historia.Status = dto.Status;
            historia.Tipo = dto.Tipo;
            historia.DataInicio = dto.DataInicio ?? historia.DataInicio;
            historia.DataFinalizacao = dto.DataFinalizacao;
            historia.Observacoes = dto.Observacoes;
            historia.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.Historias.UpdateAsync(historia);
            await _unitOfWork.SaveChangesAsync();

            if (statusAnterior != dto.Status)
            {
                await _unitOfWork.HistoriaMovimentacoes.AddAsync(new HistoriaMovimentacao
                {
                    HistoriaId = historia.Id,
                    StatusAnterior = statusAnterior,
                    StatusNovo = dto.Status,
                    UsuarioId = request.RequesterUserId,
                    DataMovimentacao = DateTime.UtcNow,
                    Observacoes = "Status atualizado."
                });

                await _unitOfWork.SaveChangesAsync();
            }

            return await BuildDtoAsync(historia);
        }

        private async Task<HistoriaDto> BuildDtoAsync(Historia historia)
        {
            var cliente = await _unitOfWork.Pessoas.GetByIdAsync(historia.ClienteId);
            var produto = await _unitOfWork.Produtos.GetByIdAsync(historia.ProdutoId);
            var movimentos = await _unitOfWork.HistoriaMovimentacoes.FindAsync(m => m.HistoriaId == historia.Id);

            var usuarioIds = movimentos.Select(m => m.UsuarioId)
                .Append(historia.UsuarioResponsavelId)
                .Distinct()
                .ToList();
            var usuarios = await _unitOfWork.Users.FindAsync(u => usuarioIds.Contains(u.Id));

            var clienteLookup = cliente != null ? new Dictionary<int, Pessoa> { { cliente.Id, cliente } } : new Dictionary<int, Pessoa>();
            var produtoLookup = produto != null ? new Dictionary<int, Produto> { { produto.Id, produto } } : new Dictionary<int, Produto>();
            var usuarioLookup = usuarios.ToDictionary(u => u.Id, u => u);
            var movimentosLookup = new Dictionary<int, List<HistoriaMovimentacao>>
            {
                { historia.Id, movimentos.ToList() }
            };

            return HistoriaMapper.ToDto(historia, clienteLookup, produtoLookup, usuarioLookup, movimentosLookup);
        }
    }
}

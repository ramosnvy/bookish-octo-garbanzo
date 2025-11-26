using System.Collections.Generic;
using System.Linq;
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

            var responsavel = await _unitOfWork.Users.GetByIdAsync(dto.UsuarioResponsavelId);
            if (responsavel == null || (!request.IsGlobalAdmin && responsavel.EmpresaId != request.EmpresaId))
            {
                throw new KeyNotFoundException("Usuário responsável não encontrado para esta empresa.");
            }

            var selecoes = dto.Produtos?.Where(p => p != null).ToList() ?? new List<HistoriaProdutoInputDto>();
            if (!selecoes.Any())
            {
                throw new InvalidOperationException("Selecione ao menos um produto para atualizar a história.");
            }

            var novosProdutos = new List<HistoriaProduto>();
            foreach (var selecao in selecoes)
            {
                var produto = await _unitOfWork.Produtos.GetByIdAsync(selecao.ProdutoId);
                if (produto == null || produto.EmpresaId != request.EmpresaId)
                {
                    throw new KeyNotFoundException("Produto não encontrado para esta empresa.");
                }

                var moduloIds = (selecao.ProdutoModuloIds ?? Enumerable.Empty<int>())
                    .Where(id => id > 0)
                    .Distinct()
                    .ToList();

                if (moduloIds.Any())
                {
                    var modulos = await _unitOfWork.ProdutoModulos.FindAsync(m => moduloIds.Contains(m.Id));
                    var validModuloIds = modulos
                        .Where(m => m.ProdutoId == selecao.ProdutoId)
                        .Select(m => m.Id)
                        .Distinct()
                        .ToList();

                    if (validModuloIds.Count != moduloIds.Count)
                    {
                        throw new KeyNotFoundException("Módulo não encontrado para o produto informado.");
                    }

                    moduloIds = validModuloIds;
                }

                novosProdutos.Add(new HistoriaProduto
                {
                    HistoriaId = historia.Id,
                    ProdutoId = selecao.ProdutoId,
                    ProdutoModuloIds = moduloIds
                });
            }

            var statusAnterior = historia.Status;
            historia.ProdutoId = novosProdutos.First().ProdutoId;
            historia.UsuarioResponsavelId = dto.UsuarioResponsavelId;
            historia.Status = dto.Status;
            historia.Tipo = dto.Tipo;
            historia.DataInicio = dto.DataInicio.HasValue ? NormalizeToUtc(dto.DataInicio.Value) : historia.DataInicio;
            historia.DataFinalizacao = NormalizeToUtc(dto.DataFinalizacao);
            historia.Observacoes = dto.Observacoes;
            historia.UpdatedAt = DateTime.UtcNow;

            var existentes = await _unitOfWork.HistoriaProdutos.FindAsync(hp => hp.HistoriaId == historia.Id);
            foreach (var existente in existentes)
            {
                await _unitOfWork.HistoriaProdutos.DeleteAsync(existente);
            }

            foreach (var novo in novosProdutos)
            {
                await _unitOfWork.HistoriaProdutos.AddAsync(novo);
            }

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

        private static DateTime NormalizeToUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc),
            };
        }

        private static DateTime? NormalizeToUtc(DateTime? value) => value.HasValue ? NormalizeToUtc(value.Value) : null;

        private async Task<HistoriaDto> BuildDtoAsync(Historia historia)
        {
            var cliente = await _unitOfWork.Pessoas.GetByIdAsync(historia.ClienteId);
            var historiaProdutos = await _unitOfWork.HistoriaProdutos.FindAsync(hp => hp.HistoriaId == historia.Id);
            var produtoIds = historiaProdutos.Select(p => p.ProdutoId).Append(historia.ProdutoId).Distinct().ToList();
            var produtos = await _unitOfWork.Produtos.FindAsync(p => produtoIds.Contains(p.Id));
            var movimentos = await _unitOfWork.HistoriaMovimentacoes.FindAsync(m => m.HistoriaId == historia.Id);
            var moduloIds = historiaProdutos.SelectMany(hp => hp.ProdutoModuloIds).Distinct().ToList();
            var modulos = moduloIds.Any()
                ? await _unitOfWork.ProdutoModulos.FindAsync(m => moduloIds.Contains(m.Id))
                : Enumerable.Empty<ProdutoModulo>();

            var usuarioIds = movimentos.Select(m => m.UsuarioId)
                .Append(historia.UsuarioResponsavelId)
                .Distinct()
                .ToList();
            var usuarios = await _unitOfWork.Users.FindAsync(u => usuarioIds.Contains(u.Id));

            var clienteLookup = cliente != null ? new Dictionary<int, Pessoa> { { cliente.Id, cliente } } : new Dictionary<int, Pessoa>();
            var produtoLookup = produtos.ToDictionary(p => p.Id, p => p);
            var moduloLookup = modulos.ToDictionary(m => m.Id, m => m);
            var usuarioLookup = usuarios.ToDictionary(u => u.Id, u => u);
            var movimentosLookup = new Dictionary<int, List<HistoriaMovimentacao>>
            {
                { historia.Id, movimentos.ToList() }
            };
            var produtosLookup = new Dictionary<int, List<HistoriaProduto>>
            {
                { historia.Id, historiaProdutos.ToList() }
            };

            return HistoriaMapper.ToDto(
                historia,
                clienteLookup,
                produtoLookup,
                moduloLookup,
                usuarioLookup,
                produtosLookup,
                movimentosLookup);
        }
    }
}

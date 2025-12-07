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
                throw new KeyNotFoundException("História não encontrada.");
            }

            var cliente = await _unitOfWork.Pessoas.GetByIdAsync(historia.ClienteId);
            if (cliente == null || cliente.Tipo != PessoaTipo.Cliente || cliente.EmpresaId != request.EmpresaId)
            {
                throw new UnauthorizedAccessException("História não pertence à empresa informada.");
            }

            User? responsavel = null;
            if (dto.UsuarioResponsavelId.HasValue)
            {
                responsavel = await _unitOfWork.Users.GetByIdAsync(dto.UsuarioResponsavelId.Value);
                if (responsavel == null || (!request.IsGlobalAdmin && responsavel.EmpresaId != request.EmpresaId))
                {
                    throw new KeyNotFoundException("Usuário responsável não encontrado para esta empresa.");
                }
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

            var status = await GetStatusAsync(dto.StatusId, request.EmpresaId);
            var tipo = await GetTipoAsync(dto.TipoId, request.EmpresaId);
            var statusAnteriorId = historia.HistoriaStatusId;

            historia.ProdutoId = novosProdutos.First().ProdutoId;
            historia.UsuarioResponsavelId = dto.UsuarioResponsavelId;
            historia.HistoriaStatusId = status.Id;
            historia.HistoriaTipoId = tipo.Id;
            historia.PrevisaoDias = dto.PrevisaoDias;
            historia.Observacoes = dto.Observacoes;
            historia.UpdatedAt = DateTime.UtcNow;

            if (status.FechaHistoria && !historia.DataFinalizacao.HasValue)
            {
                historia.DataFinalizacao = DateTime.UtcNow;
            }
            else if (!status.FechaHistoria)
            {
                historia.DataFinalizacao = null;
            }

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

            if (statusAnteriorId != status.Id)
            {
                await _unitOfWork.HistoriaMovimentacoes.AddAsync(new HistoriaMovimentacao
                {
                    HistoriaId = historia.Id,
                    StatusAnteriorId = statusAnteriorId,
                    StatusNovoId = status.Id,
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
            var historiaProdutos = await _unitOfWork.HistoriaProdutos.FindAsync(hp => hp.HistoriaId == historia.Id);
            var produtoIds = historiaProdutos
                .Select(p => p.ProdutoId)
                .Concat(new[] { historia.ProdutoId })
                .Distinct()
                .ToList();
            var produtos = await _unitOfWork.Produtos.FindAsync(p => produtoIds.Contains(p.Id));
            var movimentos = await _unitOfWork.HistoriaMovimentacoes.FindAsync(m => m.HistoriaId == historia.Id);
            var moduloIds = historiaProdutos.SelectMany(hp => hp.ProdutoModuloIds).Distinct().ToList();
            var modulos = moduloIds.Any()
                ? await _unitOfWork.ProdutoModulos.FindAsync(m => moduloIds.Contains(m.Id))
                : Enumerable.Empty<ProdutoModulo>();

            var statusIds = movimentos.SelectMany(m => new[] { m.StatusAnteriorId, m.StatusNovoId })
                .Concat(new[] { historia.HistoriaStatusId })
                .Distinct()
                .ToList();
            var statuses = statusIds.Any()
                ? await _unitOfWork.HistoriaStatuses.FindAsync(s => statusIds.Contains(s.Id))
                : Enumerable.Empty<HistoriaStatus>();
            var statusLookup = statuses.ToDictionary(s => s.Id, s => s);

            var tipoIds = new[] { historia.HistoriaTipoId };
            var tipos = await _unitOfWork.HistoriaTipos.FindAsync(t => tipoIds.Contains(t.Id));
            var tipoLookup = tipos.ToDictionary(t => t.Id, t => t);

            var usuarioIds = movimentos.Select(m => m.UsuarioId)
                .Concat(historia.UsuarioResponsavelId.HasValue
                    ? new[] { historia.UsuarioResponsavelId.Value }
                    : Enumerable.Empty<int>())
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
                statusLookup,
                tipoLookup,
                produtosLookup,
                movimentosLookup);
        }

        private async Task<HistoriaStatus> GetStatusAsync(int statusId, int empresaId)
        {
            var status = await _unitOfWork.HistoriaStatuses.GetByIdAsync(statusId);
            if (status == null || (status.EmpresaId.HasValue && status.EmpresaId != empresaId))
            {
                throw new KeyNotFoundException("Status da história não encontrado para esta empresa.");
            }

            return status;
        }

        private async Task<HistoriaTipo> GetTipoAsync(int tipoId, int empresaId)
        {
            var tipo = await _unitOfWork.HistoriaTipos.GetByIdAsync(tipoId);
            if (tipo == null || (tipo.EmpresaId.HasValue && tipo.EmpresaId != empresaId))
            {
                throw new KeyNotFoundException("Tipo da história não encontrado para esta empresa.");
            }

            return tipo;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
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
                throw new InvalidOperationException("Selecione ao menos um produto para cadastrar a história.");
            }

            var historiaProdutos = new List<HistoriaProduto>();
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

                historiaProdutos.Add(new HistoriaProduto
                {
                    ProdutoId = selecao.ProdutoId,
                    ProdutoModuloIds = moduloIds
                });
            }

            var status = await GetStatusAsync(dto.StatusId, request.EmpresaId);
            var tipo = await GetTipoAsync(dto.TipoId, request.EmpresaId);

            var historia = new Historia
            {
                ClienteId = dto.ClienteId,
                ProdutoId = historiaProdutos.First().ProdutoId,
                HistoriaStatusId = status.Id,
                HistoriaTipoId = tipo.Id,
                UsuarioResponsavelId = dto.UsuarioResponsavelId,
                PrevisaoDias = dto.PrevisaoDias,
                DataInicio = DateTime.UtcNow,
                Observacoes = dto.Observacoes,
                CreatedAt = DateTime.UtcNow,
                Produtos = historiaProdutos
            };

            await _unitOfWork.Historias.AddAsync(historia);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.HistoriaMovimentacoes.AddAsync(new HistoriaMovimentacao
            {
                HistoriaId = historia.Id,
                StatusAnteriorId = status.Id,
                StatusNovoId = status.Id,
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
            var produtoIds = historia.Produtos
                .Select(p => p.ProdutoId)
                .Concat(new[] { historia.ProdutoId })
                .Distinct()
                .ToList();
            var produtos = await _unitOfWork.Produtos.FindAsync(p => produtoIds.Contains(p.Id));
            var movimentos = await _unitOfWork.HistoriaMovimentacoes.FindAsync(m => m.HistoriaId == historia.Id);
            var historiaProdutos = await _unitOfWork.HistoriaProdutos.FindAsync(hp => hp.HistoriaId == historia.Id);
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

            var clienteLookup = clientes.ToDictionary(c => c.Id, c => c);
            var produtoLookup = produtos.ToDictionary(p => p.Id, p => p);
            var moduloLookup = modulos.ToDictionary(m => m.Id, m => m);
            var usuarioIds = movimentos.Select(m => m.UsuarioId)
                .Concat(historia.UsuarioResponsavelId.HasValue
                    ? new[] { historia.UsuarioResponsavelId.Value }
                    : Enumerable.Empty<int>())
                .Distinct()
                .ToList();
            var usuarios = await _unitOfWork.Users.FindAsync(u => usuarioIds.Contains(u.Id));
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

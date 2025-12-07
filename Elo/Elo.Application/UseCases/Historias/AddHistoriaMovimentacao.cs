using System.Collections.Generic;
using System.Linq;
using MediatR;
using Elo.Application.DTOs.Historia;
using Elo.Domain.Entities;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.Historias;

public static class AddHistoriaMovimentacao
{
    public class Command : IRequest<HistoriaDto>
    {
        public int HistoriaId { get; set; }
        public int EmpresaId { get; set; }
        public int UsuarioId { get; set; }
        public CreateHistoriaMovimentacaoDto Dto { get; set; } = new();
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
            var historia = await _unitOfWork.Historias.GetByIdAsync(request.HistoriaId);
            if (historia == null)
            {
                throw new KeyNotFoundException("História não encontrada.");
            }

            var cliente = await _unitOfWork.Pessoas.GetByIdAsync(historia.ClienteId);
            if (cliente == null || cliente.Tipo != PessoaTipo.Cliente || cliente.EmpresaId != request.EmpresaId)
            {
                throw new UnauthorizedAccessException("História não pertence à empresa informada.");
            }

            var statusAnteriorId = historia.HistoriaStatusId;
            var statusAnteriorExists = await _unitOfWork.HistoriaStatuses.ExistsAsync(s => s.Id == statusAnteriorId);
            if (!statusAnteriorExists)
            {
                throw new KeyNotFoundException("Status anterior da história não encontrado.");
            }

            var statusNovo = await GetStatusAsync(request.Dto.StatusNovoId, request.EmpresaId);

            historia.HistoriaStatusId = statusNovo.Id;
            if (statusNovo.FechaHistoria && !historia.DataFinalizacao.HasValue)
            {
                historia.DataFinalizacao = DateTime.UtcNow;
            }
            else if (!statusNovo.FechaHistoria)
            {
                historia.DataFinalizacao = null;
            }

            historia.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Historias.UpdateAsync(historia);

            await _unitOfWork.HistoriaMovimentacoes.AddAsync(new HistoriaMovimentacao
            {
                HistoriaId = historia.Id,
                StatusAnteriorId = statusAnteriorId,
                StatusNovoId = statusNovo.Id,
                UsuarioId = request.UsuarioId,
                DataMovimentacao = DateTime.UtcNow,
                Observacoes = request.Dto.Observacoes
            });

            await _unitOfWork.SaveChangesAsync();

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
            var produtos = produtoIds.Any()
                ? await _unitOfWork.Produtos.FindAsync(p => produtoIds.Contains(p.Id))
                : Enumerable.Empty<Produto>();
            var moduloIds = historiaProdutos.SelectMany(p => p.ProdutoModuloIds).Distinct().ToList();
            var modulos = moduloIds.Any()
                ? await _unitOfWork.ProdutoModulos.FindAsync(m => moduloIds.Contains(m.Id))
                : Enumerable.Empty<ProdutoModulo>();
            var movimentos = await _unitOfWork.HistoriaMovimentacoes.FindAsync(m => m.HistoriaId == historia.Id);

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
    }
}

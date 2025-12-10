using Elo.Domain.Entities;
using Elo.Domain.Enums;
using Elo.Domain.Exceptions;
using Elo.Domain.Interfaces;
using Elo.Domain.Interfaces.Repositories;
using Elo.Domain.Models;
using System.Linq;
using System.Collections.Generic;

namespace Elo.Domain.Services;

public class ProdutoService : IProdutoService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProdutoService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Produto> CriarProdutoAsync(string nome, string descricao, decimal valorCusto, decimal valorRevenda, bool ativo, int? fornecedorId, IEnumerable<ProdutoModuloInput> modulos, int empresaId)
    {
        // Validações de negócio
        if (valorCusto <= 0)
        {
            throw new ClienteJaExisteException("Valor de custo deve ser maior que zero");
        }

        if (valorRevenda <= 0)
        {
            throw new ClienteJaExisteException("Valor de revenda deve ser maior que zero");
        }

        if (valorRevenda <= valorCusto)
        {
            throw new ClienteJaExisteException("Valor de revenda deve ser maior que o valor de custo");
        }

        var fornecedor = await ObterFornecedorValidoAsync(fornecedorId, empresaId);

        // Criação da entidade
        var produto = new Produto
        {
            EmpresaId = empresaId,
            Nome = nome,
            Descricao = descricao,
            ValorCusto = valorCusto,
            ValorRevenda = valorRevenda,
            MargemLucro = CalcularMargemLucro(valorCusto, valorRevenda),
            Ativo = ativo,
            CreatedAt = DateTime.UtcNow,
            FornecedorId = fornecedor?.Id,
            Fornecedor = fornecedor
        };

        await _unitOfWork.Produtos.AddAsync(produto);
        await _unitOfWork.SaveChangesAsync();

        await SincronizarModulosAsync(produto, modulos);

        return produto;
    }

    public async Task<Produto> AtualizarProdutoAsync(int id, string nome, string descricao, decimal valorCusto, decimal valorRevenda, bool ativo, int? fornecedorId, IEnumerable<ProdutoModuloInput> modulos, int empresaId)
    {
        var produto = await _unitOfWork.Produtos.GetByIdAsync(id);
        if (produto == null || produto.EmpresaId != empresaId)
        {
            throw new ClienteNaoEncontradoException(id);
        }

        // Validações de negócio
        if (valorCusto <= 0)
        {
            throw new ClienteJaExisteException("Valor de custo deve ser maior que zero");
        }

        if (valorRevenda <= 0)
        {
            throw new ClienteJaExisteException("Valor de revenda deve ser maior que zero");
        }

        if (valorRevenda <= valorCusto)
        {
            throw new ClienteJaExisteException("Valor de revenda deve ser maior que o valor de custo");
        }

        var fornecedor = await ObterFornecedorValidoAsync(fornecedorId, empresaId);

        // Atualização da entidade
        produto.Nome = nome;
        produto.Descricao = descricao;
        produto.ValorCusto = valorCusto;
        produto.ValorRevenda = valorRevenda;
        produto.MargemLucro = CalcularMargemLucro(valorCusto, valorRevenda);
        produto.Ativo = ativo;
        produto.UpdatedAt = DateTime.UtcNow;
        produto.FornecedorId = fornecedor?.Id;
        produto.Fornecedor = fornecedor;

        await _unitOfWork.Produtos.UpdateAsync(produto);
        await _unitOfWork.SaveChangesAsync();

        await SincronizarModulosAsync(produto, modulos);

        return produto;
    }

    public async Task<bool> DeletarProdutoAsync(int id, int empresaId)
    {
        var produto = await _unitOfWork.Produtos.GetByIdAsync(id);
        if (produto == null || produto.EmpresaId != empresaId)
        {
            throw new ClienteNaoEncontradoException(id);
        }

        await _unitOfWork.Produtos.DeleteAsync(produto);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<Produto?> ObterProdutoPorIdAsync(int id, int? empresaId = null)
    {
        var produto = await _unitOfWork.Produtos.GetByIdAsync(id);
        if (produto == null)
        {
            return null;
        }

        if (empresaId.HasValue && produto.EmpresaId != empresaId.Value)
        {
            return null;
        }

        await CarregarFornecedoresAsync(new[] { produto }, empresaId);
        await CarregarModulosAsync(new[] { produto });
        return produto;
    }

    public async Task<IEnumerable<Produto>> ObterTodosProdutosAsync(int? empresaId = null)
    {
        var produtos = (await _unitOfWork.Produtos.FindAsync(p => empresaId.HasValue ? p.EmpresaId == empresaId : true)).ToList();
        await CarregarFornecedoresAsync(produtos, empresaId);
        await CarregarModulosAsync(produtos);
        return produtos;
    }

    public async Task<IEnumerable<Produto>> ObterProdutosPorIdsAsync(IEnumerable<int> ids)
    {
        if (!ids.Any()) return Enumerable.Empty<Produto>();
        var idList = ids.Distinct().ToList();
        var produtos = (await _unitOfWork.Produtos.FindAsync(p => idList.Contains(p.Id))).ToList();
        await CarregarFornecedoresAsync(produtos, null);
        await CarregarModulosAsync(produtos);
        return produtos;
    }

    public async Task<IEnumerable<ProdutoModulo>> ObterModulosPorIdsAsync(IEnumerable<int> ids)
    {
        if (!ids.Any()) return Enumerable.Empty<ProdutoModulo>();
        var idList = ids.Distinct().ToList();
        return await _unitOfWork.ProdutoModulos.FindAsync(m => idList.Contains(m.Id));
    }

    public decimal CalcularMargemLucro(decimal valorCusto, decimal valorRevenda)
    {
        if (valorCusto <= 0 || valorRevenda <= 0)
        {
            return 0;
        }

        return ((valorRevenda - valorCusto) / valorCusto) * 100;
    }
    private async Task<Pessoa?> ObterFornecedorValidoAsync(int? fornecedorId, int empresaId)
    {
        if (!fornecedorId.HasValue)
        {
            return null;
        }

        var fornecedor = await _unitOfWork.Pessoas.GetByIdAsync(fornecedorId.Value);
        if (fornecedor == null || fornecedor.Tipo != PessoaTipo.Fornecedor || fornecedor.EmpresaId != empresaId)
        {
            throw new ClienteNaoEncontradoException(fornecedorId.Value);
        }

        return fornecedor;
    }

    private async Task CarregarFornecedoresAsync(IEnumerable<Produto> produtos, int? empresaId)
    {
        var fornecedorIds = produtos
            .Where(p => p.FornecedorId.HasValue)
            .Select(p => p.FornecedorId!.Value)
            .Distinct()
            .ToList();

        if (!fornecedorIds.Any())
        {
            return;
        }

        var fornecedores = await _unitOfWork.Pessoas.FindAsync(p => fornecedorIds.Contains(p.Id) && (!empresaId.HasValue || p.EmpresaId == empresaId));
        var fornecedoresLookup = fornecedores
            .Where(f => f.Tipo == PessoaTipo.Fornecedor)
            .ToDictionary(f => f.Id, f => f);

        foreach (var produto in produtos)
        {
            if (produto.FornecedorId.HasValue && fornecedoresLookup.TryGetValue(produto.FornecedorId.Value, out var fornecedor))
            {
                produto.Fornecedor = fornecedor;
            }
        }
    }

    private async Task SincronizarModulosAsync(Produto produto, IEnumerable<ProdutoModuloInput> modulosInput)
    {
        var inputs = (modulosInput ?? Enumerable.Empty<ProdutoModuloInput>())
            .Where(m => !string.IsNullOrWhiteSpace(m.Nome))
            .ToList();

        var existentes = await _unitOfWork.ProdutoModulos.FindAsync(m => m.ProdutoId == produto.Id);

        foreach (var modulo in existentes)
        {
            await _unitOfWork.ProdutoModulos.DeleteAsync(modulo);
        }

        var novosModulos = new List<ProdutoModulo>();

        foreach (var input in inputs)
        {
            var modulo = new ProdutoModulo
            {
                ProdutoId = produto.Id,
                Nome = input.Nome,
                Descricao = input.Descricao,
                ValorAdicional = input.ValorAdicional,
                CustoAdicional = input.CustoAdicional,
                Ativo = input.Ativo,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.ProdutoModulos.AddAsync(modulo);
            novosModulos.Add(modulo);
        }

        if (inputs.Any() || existentes.Any())
        {
            await _unitOfWork.SaveChangesAsync();
        }

        produto.Modulos = novosModulos;
    }

    private async Task CarregarModulosAsync(IEnumerable<Produto> produtos)
    {
        var ids = produtos.Select(p => p.Id).ToList();
        if (!ids.Any())
        {
            return;
        }

        var modulos = await _unitOfWork.ProdutoModulos.FindAsync(m => ids.Contains(m.ProdutoId));
        var lookup = modulos.GroupBy(m => m.ProdutoId).ToDictionary(g => g.Key, g => g.ToList());

        foreach (var produto in produtos)
        {
            if (lookup.TryGetValue(produto.Id, out var lista))
            {
                produto.Modulos = lista;
            }
            else
            {
                produto.Modulos = new List<ProdutoModulo>();
            }
        }
    }
}

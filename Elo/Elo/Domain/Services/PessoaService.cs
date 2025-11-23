using Elo.Domain.Entities;
using Elo.Domain.Enums;
using Elo.Domain.Exceptions;
using Elo.Domain.Interfaces;
using Elo.Domain.Interfaces.Repositories;
using System.Linq;

namespace Elo.Domain.Services;

public class PessoaService : IPessoaService
{
    private readonly IUnitOfWork _unitOfWork;

    public PessoaService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Pessoa> CriarPessoaAsync(PessoaTipo tipo, string nome, string documento, string email, string telefone, Status status, int? categoriaId, IEnumerable<PessoaEndereco> enderecos, int empresaId)
    {
        await ValidarDuplicidadesAsync(documento, email, empresaId);
        var categoria = await ObterCategoriaAsync(tipo, categoriaId, empresaId);

        var pessoa = new Pessoa
        {
            EmpresaId = empresaId,
            Nome = nome,
            Documento = documento,
            Email = email,
            Telefone = telefone,
            Categoria = categoria?.Nome ?? string.Empty,
            FornecedorCategoriaId = categoria?.Id,
            Status = status,
            Tipo = tipo,
            DataCadastro = DateTime.UtcNow
        };

        await _unitOfWork.Pessoas.AddAsync(pessoa);
        await _unitOfWork.SaveChangesAsync();

        pessoa.FornecedorCategoria = categoria;
        await SincronizarEnderecosAsync(pessoa.Id, enderecos);

        return pessoa;
    }

    public async Task<Pessoa> AtualizarPessoaAsync(int id, PessoaTipo tipo, string nome, string documento, string email, string telefone, Status status, int? categoriaId, IEnumerable<PessoaEndereco> enderecos, int empresaId)
    {
        var pessoa = await _unitOfWork.Pessoas.GetByIdAsync(id);
        if (pessoa == null || pessoa.Tipo != tipo || pessoa.EmpresaId != empresaId)
        {
            throw new ClienteNaoEncontradoException(id);
        }

        await ValidarDuplicidadesAsync(documento, email, empresaId, id);
        var categoria = await ObterCategoriaAsync(tipo, categoriaId, empresaId);

        pessoa.Nome = nome;
        pessoa.Documento = documento;
        pessoa.Email = email;
        pessoa.Telefone = telefone;
        pessoa.Categoria = categoria?.Nome ?? string.Empty;
        pessoa.FornecedorCategoriaId = categoria?.Id;
        pessoa.FornecedorCategoria = categoria;
        pessoa.Status = status;
        pessoa.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Pessoas.UpdateAsync(pessoa);
        await _unitOfWork.SaveChangesAsync();

        await SincronizarEnderecosAsync(pessoa.Id, enderecos);

        return pessoa;
    }

    public async Task<bool> DeletarPessoaAsync(int id, PessoaTipo tipo, int empresaId)
    {
        var pessoa = await _unitOfWork.Pessoas.GetByIdAsync(id);
        if (pessoa == null || pessoa.Tipo != tipo || pessoa.EmpresaId != empresaId)
        {
            throw new ClienteNaoEncontradoException(id);
        }

        await _unitOfWork.Pessoas.DeleteAsync(pessoa);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<Pessoa?> ObterPessoaPorIdAsync(int id, PessoaTipo tipo, int? empresaId = null)
    {
        var pessoa = await _unitOfWork.Pessoas.GetByIdAsync(id);
        if (pessoa == null || pessoa.Tipo != tipo)
        {
            return null;
        }

        if (empresaId.HasValue && pessoa.EmpresaId != empresaId.Value)
        {
            return null;
        }

        var enderecos = await _unitOfWork.PessoaEnderecos.FindAsync(e => e.PessoaId == id);
        pessoa.Enderecos = enderecos.ToList();
        if (pessoa.FornecedorCategoriaId.HasValue)
        {
            pessoa.FornecedorCategoria = await _unitOfWork.FornecedorCategorias.GetByIdAsync(pessoa.FornecedorCategoriaId.Value);
        }

        return pessoa;
    }

    public async Task<IEnumerable<Pessoa>> ObterPessoasAsync(PessoaTipo tipo, int? empresaId = null)
    {
        var pessoas = (await _unitOfWork.Pessoas.FindAsync(p => empresaId.HasValue ? p.Tipo == tipo && p.EmpresaId == empresaId : p.Tipo == tipo)).ToList();
        var pessoaIds = pessoas.Select(p => p.Id).ToList();
        var enderecos = pessoaIds.Any()
            ? await _unitOfWork.PessoaEnderecos.FindAsync(e => pessoaIds.Contains(e.PessoaId))
            : Enumerable.Empty<PessoaEndereco>();
        var lookup = enderecos.GroupBy(e => e.PessoaId).ToDictionary(g => g.Key, g => g.ToList());
        var categorias = empresaId.HasValue
            ? await _unitOfWork.FornecedorCategorias.FindAsync(c => c.EmpresaId == empresaId)
            : await _unitOfWork.FornecedorCategorias.GetAllAsync();
        var categoriasLookup = categorias.ToDictionary(c => c.Id, c => c);

        foreach (var pessoa in pessoas)
        {
            if (lookup.TryGetValue(pessoa.Id, out var lista))
            {
                pessoa.Enderecos = lista;
            }

            if (pessoa.FornecedorCategoriaId.HasValue && categoriasLookup.TryGetValue(pessoa.FornecedorCategoriaId.Value, out var categoria))
            {
                pessoa.FornecedorCategoria = categoria;
            }
        }

        return pessoas;
    }

    private async Task ValidarDuplicidadesAsync(string documento, string email, int empresaId, int? pessoaId = null)
    {
        var documentoExistente = await _unitOfWork.Pessoas.FirstOrDefaultAsync(p => p.Documento == documento && p.EmpresaId == empresaId);
        if (documentoExistente != null && documentoExistente.Id != pessoaId)
        {
            throw new ClienteJaExisteException("Já existe uma pessoa cadastrada com este documento.");
        }

        var emailExistente = await _unitOfWork.Pessoas.FirstOrDefaultAsync(p => p.Email == email && p.EmpresaId == empresaId);
        if (emailExistente != null && emailExistente.Id != pessoaId)
        {
            throw new ClienteJaExisteException("Já existe uma pessoa cadastrada com este email.");
        }
    }

    private async Task SincronizarEnderecosAsync(int pessoaId, IEnumerable<PessoaEndereco> enderecos)
    {
        var existentes = await _unitOfWork.PessoaEnderecos.FindAsync(e => e.PessoaId == pessoaId);
        foreach (var existente in existentes)
        {
            await _unitOfWork.PessoaEnderecos.DeleteAsync(existente);
        }

        if (enderecos != null)
        {
            foreach (var endereco in enderecos)
            {
                if (string.IsNullOrWhiteSpace(endereco.Logradouro))
                {
                    continue;
                }

                await _unitOfWork.PessoaEnderecos.AddAsync(new PessoaEndereco
                {
                    PessoaId = pessoaId,
                    Logradouro = endereco.Logradouro,
                    Numero = endereco.Numero,
                    Bairro = endereco.Bairro,
                    Cidade = endereco.Cidade,
                    Estado = endereco.Estado,
                    Cep = endereco.Cep,
                    Complemento = endereco.Complemento
                });
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<FornecedorCategoria?> ObterCategoriaAsync(PessoaTipo tipo, int? categoriaId, int empresaId)
    {
        if (tipo != PessoaTipo.Fornecedor)
        {
            return null;
        }

        if (!categoriaId.HasValue)
        {
            throw new ClienteJaExisteException("Fornecedores precisam estar associados a uma categoria.");
        }

        var categoria = await _unitOfWork.FornecedorCategorias.GetByIdAsync(categoriaId.Value);
        if (categoria == null || !categoria.Ativo || categoria.EmpresaId != empresaId)
        {
            throw new FornecedorCategoriaNaoEncontradaException(categoriaId.Value);
        }

        return categoria;
    }
}

using Elo.Domain.Entities;
using Elo.Domain.Enums;
using Elo.Domain.Exceptions;
using Elo.Domain.Interfaces;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Domain.Services;

public class AfiliadoService : IAfiliadoService
{
    private readonly IUnitOfWork _unitOfWork;

    public AfiliadoService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Afiliado> CriarAfiliadoAsync(string nome, string email, string documento, string telefone, decimal porcentagem, Status status, int empresaId)
    {
        // Validar duplicidades
        await ValidarDuplicidadesAsync(email, documento, empresaId);

        // Criar entidade
        var afiliado = new Afiliado
        {
            Nome = nome,
            Email = email,
            Documento = documento,
            Telefone = telefone,
            Porcentagem = porcentagem,
            Status = status,
            EmpresaId = empresaId,
            CreatedAt = DateTime.UtcNow
        };

        // Persistir
        var created = await _unitOfWork.Afiliados.AddAsync(afiliado);
        await _unitOfWork.SaveChangesAsync();

        return created;
    }

    public async Task<Afiliado> AtualizarAfiliadoAsync(int id, string nome, string email, string documento, string telefone, decimal porcentagem, Status status, int empresaId)
    {
        // Buscar afiliado existente
        var afiliado = await _unitOfWork.Afiliados.GetByIdAsync(id);
        if (afiliado == null)
            throw new AfiliadoNaoEncontradoException(id);

        // Validar empresa
        if (afiliado.EmpresaId != empresaId)
            throw new UnauthorizedAccessException("Afiliado não pertence à empresa informada");

        // Validar duplicidades (exceto o próprio afiliado)
        await ValidarDuplicidadesAsync(email, documento, empresaId, id);

        // Atualizar dados
        afiliado.Nome = nome;
        afiliado.Email = email;
        afiliado.Documento = documento;
        afiliado.Telefone = telefone;
        afiliado.Porcentagem = porcentagem;
        afiliado.Status = status;
        afiliado.UpdatedAt = DateTime.UtcNow;

        // Persistir
        await _unitOfWork.Afiliados.UpdateAsync(afiliado);
        await _unitOfWork.SaveChangesAsync();

        return afiliado;
    }

    public async Task<Afiliado> AtualizarStatusAfiliadoAsync(int id, Status status, int empresaId)
    {
        var afiliado = await _unitOfWork.Afiliados.GetByIdAsync(id);
        if (afiliado == null)
            throw new AfiliadoNaoEncontradoException(id);

        if (afiliado.EmpresaId != empresaId)
            throw new UnauthorizedAccessException("Afiliado não pertence à empresa informada");

        afiliado.Status = status;
        afiliado.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Afiliados.UpdateAsync(afiliado);
        await _unitOfWork.SaveChangesAsync();

        return afiliado;
    }

    public async Task<bool> DeletarAfiliadoAsync(int id, int empresaId)
    {
        // Buscar afiliado
        var afiliado = await _unitOfWork.Afiliados.GetByIdAsync(id);
        if (afiliado == null)
            throw new AfiliadoNaoEncontradoException(id);

        // Validar empresa
        if (afiliado.EmpresaId != empresaId)
            throw new UnauthorizedAccessException("Afiliado não pertence à empresa informada");

        // Deletar
        await _unitOfWork.Afiliados.DeleteAsync(afiliado);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<Afiliado?> ObterAfiliadoPorIdAsync(int id, int empresaId)
    {
        var afiliado = await _unitOfWork.Afiliados.GetByIdAsync(id);
        
        if (afiliado == null)
            return null;

        // Validar empresa
        if (afiliado.EmpresaId != empresaId)
            throw new UnauthorizedAccessException("Afiliado não pertence à empresa informada");

        return afiliado;
    }

    public async Task<IEnumerable<Afiliado>> ObterAfiliadosAsync(int? empresaId = null)
    {
        if (empresaId.HasValue)
        {
            return await _unitOfWork.Afiliados.FindAsync(a => a.EmpresaId == empresaId.Value);
        }

        return await _unitOfWork.Afiliados.GetAllAsync();
    }

    private async Task ValidarDuplicidadesAsync(string email, string documento, int empresaId, int? afiliadoIdExcluir = null)
    {
        var existentes = await _unitOfWork.Afiliados.FindAsync(a =>
            a.EmpresaId == empresaId &&
            (a.Email == email || a.Documento == documento));

        // Excluir o próprio afiliado se estiver atualizando
        if (afiliadoIdExcluir.HasValue)
        {
            existentes = existentes.Where(a => a.Id != afiliadoIdExcluir.Value);
        }

        var existente = existentes.FirstOrDefault();
        if (existente != null)
        {
            if (existente.Email == email)
                throw new AfiliadoJaExisteException($"Já existe um afiliado com o email {email}");
            if (existente.Documento == documento)
                throw new AfiliadoJaExisteException($"Já existe um afiliado com o documento {documento}");
        }
    }
}

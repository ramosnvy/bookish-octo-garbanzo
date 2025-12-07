using System;
using MediatR;
using Elo.Application.DTOs.Historia;
using Elo.Domain.Entities;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.HistoriaTipos;

public static class UpdateHistoriaTipo
{
    public class Command : IRequest<HistoriaTipoDto>
    {
        public int EmpresaId { get; set; }
        public UpdateHistoriaTipoDto Dto { get; set; } = new();
    }

    public class Handler : IRequestHandler<Command, HistoriaTipoDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<HistoriaTipoDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var tipo = await _unitOfWork.HistoriaTipos.GetByIdAsync(request.Dto.Id);
            if (tipo == null || tipo.EmpresaId != request.EmpresaId)
            {
                throw new KeyNotFoundException("Tipo não encontrado para esta empresa.");
            }

            var duplicate = await _unitOfWork.HistoriaTipos.FirstOrDefaultAsync(t =>
                t.Nome == request.Dto.Nome &&
                t.Id != request.Dto.Id &&
                t.EmpresaId == request.EmpresaId);

            if (duplicate != null)
            {
                throw new InvalidOperationException("Já existe um tipo com o mesmo nome para esta empresa.");
            }

            tipo.Nome = request.Dto.Nome;
            tipo.Descricao = request.Dto.Descricao;
            tipo.Ordem = request.Dto.Ordem;
            tipo.Ativo = request.Dto.Ativo;
            tipo.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.HistoriaTipos.UpdateAsync(tipo);
            await _unitOfWork.SaveChangesAsync();

            return new HistoriaTipoDto
            {
                Id = tipo.Id,
                Nome = tipo.Nome,
                Descricao = tipo.Descricao,
                Ordem = tipo.Ordem,
                Ativo = tipo.Ativo,
                CreatedAt = tipo.CreatedAt,
                UpdatedAt = tipo.UpdatedAt
            };
        }
    }
}

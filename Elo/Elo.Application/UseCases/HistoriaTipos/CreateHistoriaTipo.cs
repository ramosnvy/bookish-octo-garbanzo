using System;
using MediatR;
using Elo.Application.DTOs.Historia;
using Elo.Domain.Entities;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.HistoriaTipos;

public static class CreateHistoriaTipo
{
    public class Command : IRequest<HistoriaTipoDto>
    {
        public int EmpresaId { get; set; }
        public CreateHistoriaTipoDto Dto { get; set; } = new();
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
            var existing = await _unitOfWork.HistoriaTipos.FirstOrDefaultAsync(t =>
                t.Nome == request.Dto.Nome && t.EmpresaId == request.EmpresaId);

            if (existing != null)
            {
                throw new InvalidOperationException("Já existe um tipo com o mesmo nome para esta empresa.");
            }

            var tipo = new HistoriaTipo
            {
                EmpresaId = request.EmpresaId,
                Nome = request.Dto.Nome,
                Descricao = request.Dto.Descricao,
                Ordem = request.Dto.Ordem,
                Ativo = request.Dto.Ativo,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.HistoriaTipos.AddAsync(tipo);
            await _unitOfWork.SaveChangesAsync();

            return new HistoriaTipoDto
            {
                Id = tipo.Id,
                Nome = tipo.Nome,
                Descricao = tipo.Descricao,
                Ordem = tipo.Ordem,
                Ativo = tipo.Ativo,
                CreatedAt = tipo.CreatedAt
            };
        }
    }
}

using System;
using MediatR;
using Elo.Application.DTOs.Historia;
using Elo.Domain.Entities;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.HistoriaStatuses;

public static class CreateHistoriaStatus
{
    public class Command : IRequest<HistoriaStatusDto>
    {
        public int EmpresaId { get; set; }
        public CreateHistoriaStatusDto Dto { get; set; } = new();
    }

    public class Handler : IRequestHandler<Command, HistoriaStatusDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<HistoriaStatusDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var existing = await _unitOfWork.HistoriaStatuses.FirstOrDefaultAsync(s =>
                s.Nome == request.Dto.Nome && s.EmpresaId == request.EmpresaId);

            if (existing != null)
            {
                throw new InvalidOperationException("Já existe um status com o mesmo nome para esta empresa.");
            }

            var status = new HistoriaStatus
            {
                EmpresaId = request.EmpresaId,
                Nome = request.Dto.Nome,
                Descricao = request.Dto.Descricao,
                Cor = request.Dto.Cor,
                FechaHistoria = request.Dto.FechaHistoria,
                Ordem = request.Dto.Ordem,
                Ativo = request.Dto.Ativo,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.HistoriaStatuses.AddAsync(status);
            await _unitOfWork.SaveChangesAsync();

            return new HistoriaStatusDto
            {
                Id = status.Id,
                Nome = status.Nome,
                Descricao = status.Descricao,
                Cor = status.Cor,
                FechaHistoria = status.FechaHistoria,
                Ordem = status.Ordem,
                Ativo = status.Ativo,
                CreatedAt = status.CreatedAt
            };
        }
    }
}

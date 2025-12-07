using System;
using MediatR;
using Elo.Application.DTOs.Historia;
using Elo.Domain.Entities;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.HistoriaStatuses;

public static class UpdateHistoriaStatus
{
    public class Command : IRequest<HistoriaStatusDto>
    {
        public int EmpresaId { get; set; }
        public UpdateHistoriaStatusDto Dto { get; set; } = new();
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
            var status = await _unitOfWork.HistoriaStatuses.GetByIdAsync(request.Dto.Id);
            if (status == null || status.EmpresaId != request.EmpresaId)
            {
                throw new KeyNotFoundException("Status não encontrado para esta empresa.");
            }

            var duplicate = await _unitOfWork.HistoriaStatuses.FirstOrDefaultAsync(s =>
                s.Nome == request.Dto.Nome &&
                s.Id != request.Dto.Id &&
                s.EmpresaId == request.EmpresaId);

            if (duplicate != null)
            {
                throw new InvalidOperationException("Já existe um status com o mesmo nome para esta empresa.");
            }

            status.Nome = request.Dto.Nome;
            status.Descricao = request.Dto.Descricao;
            status.Cor = request.Dto.Cor;
            status.FechaHistoria = request.Dto.FechaHistoria;
            status.Ordem = request.Dto.Ordem;
            status.Ativo = request.Dto.Ativo;
            status.UpdatedAt = DateTime.UtcNow;

            await _unitOfWork.HistoriaStatuses.UpdateAsync(status);
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
                CreatedAt = status.CreatedAt,
                UpdatedAt = status.UpdatedAt
            };
        }
    }
}

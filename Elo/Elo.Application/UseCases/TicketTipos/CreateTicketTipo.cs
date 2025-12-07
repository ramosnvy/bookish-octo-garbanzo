using System;
using MediatR;
using Elo.Application.DTOs.Ticket;
using Elo.Domain.Entities;
using Elo.Domain.Interfaces.Repositories;

namespace Elo.Application.UseCases.TicketTipos;

public static class CreateTicketTipo
{
    public class Command : IRequest<TicketTipoDto>
    {
        public int EmpresaId { get; set; }
        public CreateTicketTipoDto Dto { get; set; } = new();
    }

    public class Handler : IRequestHandler<Command, TicketTipoDto>
    {
        private readonly IUnitOfWork _unitOfWork;

        public Handler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<TicketTipoDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var existing = await _unitOfWork.TicketTipos.FirstOrDefaultAsync(t =>
                t.Nome == request.Dto.Nome && t.EmpresaId == request.EmpresaId);

            if (existing != null)
            {
                throw new InvalidOperationException("Já existe um tipo de ticket com o mesmo nome para esta empresa.");
            }

            var tipo = new TicketTipo
            {
                EmpresaId = request.EmpresaId,
                Nome = request.Dto.Nome,
                Descricao = request.Dto.Descricao,
                Ordem = request.Dto.Ordem,
                Ativo = request.Dto.Ativo,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.TicketTipos.AddAsync(tipo);
            await _unitOfWork.SaveChangesAsync();

            return new TicketTipoDto
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

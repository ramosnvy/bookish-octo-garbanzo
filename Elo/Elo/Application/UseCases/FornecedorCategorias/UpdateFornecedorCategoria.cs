using MediatR;
using Elo.Application.DTOs.Fornecedor;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.FornecedorCategorias;

public static class UpdateFornecedorCategoria
{
    public class Command : IRequest<FornecedorCategoriaDto>
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public bool Ativo { get; set; } = true;
        public int EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Command, FornecedorCategoriaDto>
    {
        private readonly IFornecedorCategoriaService _service;

        public Handler(IFornecedorCategoriaService service)
        {
            _service = service;
        }

        public async Task<FornecedorCategoriaDto> Handle(Command request, CancellationToken cancellationToken)
        {
            var categoria = await _service.AtualizarAsync(request.Id, request.Nome, request.Ativo, request.EmpresaId);
            return new FornecedorCategoriaDto
            {
                Id = categoria.Id,
                Nome = categoria.Nome,
                Ativo = categoria.Ativo
            };
        }
    }
}

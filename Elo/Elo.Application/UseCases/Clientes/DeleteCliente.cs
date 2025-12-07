using MediatR;
using Elo.Domain.Enums;
using Elo.Domain.Interfaces;

namespace Elo.Application.UseCases.Clientes;

public static class DeleteCliente
{
    public class Command : IRequest<bool>
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
    }

    public class Handler : IRequestHandler<Command, bool>
    {
        private readonly IPessoaService _pessoaService;

        public Handler(IPessoaService pessoaService)
        {
            _pessoaService = pessoaService;
        }

        public async Task<bool> Handle(Command request, CancellationToken cancellationToken)
        {
            return await _pessoaService.DeletarPessoaAsync(request.Id, PessoaTipo.Cliente, request.EmpresaId);
        }
    }
}

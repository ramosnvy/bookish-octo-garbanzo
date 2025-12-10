using System;
using System.Collections.Generic;
using Elo.Domain.Enums;

namespace Elo.Application.DTOs.Assinatura
{
    public class AssinaturaDto
    {
        public int Id { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNome { get; set; } = string.Empty;
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public bool IsRecorrente { get; set; }
        public int? IntervaloDias { get; set; }
        public int? RecorrenciaMeses { get; set; }
        public bool Ativo { get; set; }
        
        public bool GerarFinanceiro { get; set; }

        public bool GerarImplantacao { get; set; }
        public FormaPagamento? FormaPagamento { get; set; }
        public string? FormaPagamentoNome { get; set; }
        public int? AfiliadoId { get; set; }
        public string? AfiliadoNome { get; set; }

        public List<AssinaturaProdutoDto> Produtos { get; set; } = new();
    }

    public class AssinaturaProdutoDto
    {
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; } = string.Empty;
        public List<AssinaturaModuloDto> Modulos { get; set; } = new();
    }

    public class AssinaturaModuloDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
    }
}

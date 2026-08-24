namespace FaturamentoService.Models;

public enum StatusNotaFiscal
{
    Aberta = 0,
    Fechada = 1
}

public class NotaFiscal
{
    public int Id { get; set; }

    public int Numero { get; set; }

    public StatusNotaFiscal Status { get; set; } = StatusNotaFiscal.Aberta;

    public DateTime CriadaEm { get; set; } = DateTime.UtcNow;

    public DateTime? ImpressaEm { get; set; }

    public List<ItemNotaFiscal> Itens { get; set; } = new();
}

public class ItemNotaFiscal
    {
        public int Id { get; set; }

        public int NotaFiscalId { get; set; }

        public NotaFiscal? NotaFiscal { get; set; }

        public int ProdutoId { get; set; }

        public string ProdutoCodigo { get; set; } = string.Empty;
        public string ProdutoDescricao { get; set; } = string.Empty;

        public int Quantidade { get; set; }

 }

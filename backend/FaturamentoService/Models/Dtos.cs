namespace FaturamentoService.Models;

public record ItemNotaFiscalCreateDto(int ProdutoId, string ProdutoCodigo, string ProdutoDescricao, int Quantidade);

public record NotaFiscalCreateDto(List<ItemNotaFiscalCreateDto> Itens);

public record ItemNotaFiscalDto(int ProdutoId, string ProdutoCodigo, string ProdutoDescricao, int Quantidade);

public record NotaFiscalDto(
    int Id,
    int Numero,
    StatusNotaFiscal Status,
    DateTime CriadaEm,
    DateTime? ImpressaEm,
    List<ItemNotaFiscalDto> Itens);

public record ErrorDto(string Mensagem);
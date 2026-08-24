
namespace EstoqueService.Models;

public record ProdutoCreateDto(string Codigo, string Descricao, int Saldo);

public record ProdutoUpdateDto(string Descricao, int Saldo);

public record ProdutoDto(int Id, string Codigo, string Descricao, int Saldo);

public record MovimentoEstoqueDto(int ProdutoId, int Quantidade);

public record MovimentoEstoqueResultDto(bool Sucesso, string? Mensagem, int? SaldoAtual);


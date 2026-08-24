using System.Text.Json.Serialization;

namespace FaturamentoService.Services;

public record MovimentoEstoqueResponse(bool Sucesso, string? Mensagem, int? SaldoAtual);

public record ProdutoEstoqueDto(
    int Id,
    string Codigo,
    string Descricao,
    int Saldo
);

public interface IEstoqueClient
{
    Task<MovimentoEstoqueResponse> DarBaixaAsync(int produtoId, int quantidade, CancellationToken ct);
    Task EstornarAsync(int produtoId, int quantidade, CancellationToken ct);
    Task<ProdutoEstoqueDto?> ObterPorIdAsync(int produtoId, CancellationToken ct);
}
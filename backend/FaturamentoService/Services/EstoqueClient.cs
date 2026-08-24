using System.Net.Http.Json;
using System.Text.Json;
using FaturamentoService.Middleware;

namespace FaturamentoService.Services;

public class EstoqueClient : IEstoqueClient
{
    private readonly HttpClient _http;
    private readonly ILogger<EstoqueClient> _logger;

    public EstoqueClient(
        HttpClient http,
        ILogger<EstoqueClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<MovimentoEstoqueResponse> DarBaixaAsync(
        int produtoId,
        int quantidade,
        CancellationToken ct)
    {
        try
        {
            var response = await _http.PostAsJsonAsync(
                $"api/produtos/{produtoId}/baixa",
                new
                {
                    ProdutoId = produtoId,
                    Quantidade = quantidade
                },
                ct);

            if (response.IsSuccessStatusCode ||
                (int)response.StatusCode == 422)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var body =
                    await response.Content.ReadFromJsonAsync<MovimentoEstoqueResponse>(
                        options,
                        cancellationToken: ct);

                return body ??
                    new MovimentoEstoqueResponse(
                        false,
                        "Resposta vazia do Servico de Estoque.",
                        null);
            }

            throw new EstoqueIndisponivelException(
                $"Servico de Estoque respondeu {(int)response.StatusCode}.");
        }
        catch (HttpRequestException ex)
        {
            throw new EstoqueIndisponivelException(
                "Falha de comunicacao com o Servico de Estoque",
                ex);
        }
        catch (TaskCanceledException ex)
            when (!ct.IsCancellationRequested)
        {
            throw new EstoqueIndisponivelException(
                "Tempo limite excedido ao chamar o servico de Estoque",
                ex);
        }
    }

    public async Task EstornarAsync(
        int produtoId,
        int quantidade,
        CancellationToken ct)
    {
        try
        {
            await _http.PostAsJsonAsync(
                $"api/produtos/{produtoId}/estorno",
                new
                {
                    ProdutoId = produtoId,
                    Quantidade = quantidade
                },
                ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Falha ao Estornar saldo do produto {produtoId} (qtd {Quantidade})",
                produtoId,
                quantidade);
        }
    }

    public async Task<ProdutoEstoqueDto?> ObterPorIdAsync(
        int produtoId,
        CancellationToken ct)
    {
        try
        {
            var url = $"api/produtos/{produtoId}";

            _logger.LogInformation(
                "Consultando produto no Estoque: {Url}",
                url);

            var response = await _http.GetAsync(url, ct);

            var conteudo = await response.Content.ReadAsStringAsync(ct);

            _logger.LogInformation(
                "Resposta do Estoque: Status={StatusCode}, Conteudo={Conteudo}",
                (int)response.StatusCode,
                conteudo);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Estoque retornou HTTP {StatusCode} para produto {ProdutoId}",
                    (int)response.StatusCode,
                    produtoId);

                return null;
            }

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var produto =
                JsonSerializer.Deserialize<ProdutoEstoqueDto>(
                    conteudo,
                    options);

            if (produto != null)
            {
                _logger.LogInformation(
                    "Produto recebido: Id={Id}, Codigo={Codigo}, Saldo={Saldo}",
                    produto.Id,
                    produto.Codigo,
                    produto.Saldo);
            }

            return produto;
        }
        catch (HttpRequestException ex)
        {
            throw new EstoqueIndisponivelException(
                "Falha de comunicacao com o Servico de Estoque ao consultar produto",
                ex);
        }
        catch (TaskCanceledException ex)
            when (!ct.IsCancellationRequested)
        {
            throw new EstoqueIndisponivelException(
                "Tempo limite excedido ao consultar o produto no servico de Estoque",
                ex);
        }
    }
}
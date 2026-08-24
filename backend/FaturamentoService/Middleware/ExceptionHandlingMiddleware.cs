using System.Net;
using System.Text.Json;

namespace FaturamentoService.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (KeyNotFoundException Ex)
        {
            await WriteError(context, HttpStatusCode.NotFound, Ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await WriteError(context, HttpStatusCode.UnprocessableEntity, ex.Message);
        }
        catch (EstoqueIndisponivelException ex)
        {
            _logger.LogError(ex, "Servico de Estoque Indisponivel");
            await WriteError(context, HttpStatusCode.BadGateway,
                "Nao foi possivel concluir a impressao: o servico de estoque esta indisponivel no momento. Tente novamente em instantes");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro nao tratado no Servico de Faturamento");
            await WriteError(context, HttpStatusCode.InternalServerError, "Ocorreu um erro interno no servico de faturamento.");
        }

    }

    private static async Task WriteError(HttpContext context, HttpStatusCode statusCode, string message)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;
        var payload = JsonSerializer.Serialize(new { status = (int)statusCode, error = message, timestamp = DateTime.UtcNow });
        await context.Response.WriteAsync(payload);
    }
}

 public class EstoqueIndisponivelException : Exception
{
    public EstoqueIndisponivelException(string message, Exception? inner = null) : base(message, inner) { } 

}


public static class ExceptionHandlingMiddlewareExtensions
{
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
            => app.UseMiddleware<ExceptionHandlingMiddleware>();
}
    

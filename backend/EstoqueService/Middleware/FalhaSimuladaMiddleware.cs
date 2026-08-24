using EstoqueService.Controllers;

namespace EstoqueService.Middleware;

public class FalhaSimuladaMiddleware
{
    private readonly RequestDelegate _next;

    public FalhaSimuladaMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var isDiagnostico = context.Request.Path.StartsWithSegments("/api/diagnostico");

        if (!isDiagnostico && DiagnosticoController.SimularIndisponibilidade)
        {
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsJsonAsync(new
            {
                status = 503,
                error = "Serviço de Estoque indisponível (falha simulada)."
            });
            return;
        }

        await _next(context);
    }
}

public static class FalhaSimuladaMiddlewareExtensions
{
    public static IApplicationBuilder UseFalhaSimulada(this IApplicationBuilder app)
        => app.UseMiddleware<FalhaSimuladaMiddleware>();
}

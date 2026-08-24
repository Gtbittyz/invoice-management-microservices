namespace EstoqueService.Controllers;

using Microsoft.AspNetCore.Mvc;


[ApiController]
[Route("api/diagnostico")]
public class DiagnosticoController : ControllerBase
{
	public static bool SimularIndisponibilidade = false;

	[HttpPost("simular-falha")]
	public IActionResult SimularFalha([FromQuery] bool ativar)
	{
		SimularIndisponibilidade = ativar;
		return Ok(new { SimularFalha = SimularIndisponibilidade });
	}

	[HttpGet("Status")]
	public IActionResult Status() => Ok(new { simulandoFalha = SimularIndisponibilidade, servico = "EstoqueService" });
}
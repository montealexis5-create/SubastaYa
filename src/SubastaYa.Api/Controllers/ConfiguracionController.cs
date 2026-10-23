using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SubastaYa.Application.Configuracion;
using SubastaYa.Application.DTOs.Salida;

namespace SubastaYa.Api.Controllers;

[ApiController]
[Route("api/v1/config")]
public class ConfiguracionController : ControllerBase
{
	private readonly OpcionesPublicacion _publicacion;

	public ConfiguracionController(IOptions<OpcionesPublicacion> publicacion)
	{
		_publicacion = publicacion.Value;
	}

	[HttpGet("auctions")]
	[ProducesResponseType(typeof(ConfiguracionPublicacionResponse), StatusCodes.Status200OK)]
	public ActionResult<ConfiguracionPublicacionResponse> ObtenerDePublicacion()
		=> Ok(new ConfiguracionPublicacionResponse
		{
			DuracionMaximaDias = _publicacion.DuracionMaximaDias,
			ToleranciaInicioMinutos = _publicacion.ToleranciaInicioMinutos
		});
}

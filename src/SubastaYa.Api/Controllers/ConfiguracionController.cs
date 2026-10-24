using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.DTOs.Salida;
using SubastaYa.Application.Servicios;

namespace SubastaYa.Api.Controllers;

[ApiController]
[Route("api/v1/config")]
public class ConfiguracionController : ControllerBase
{
	private readonly IServicioDeConfiguracion _servicio;

	public ConfiguracionController(IServicioDeConfiguracion servicio)
	{
		_servicio = servicio;
	}

	[HttpGet("auctions")]
	[ProducesResponseType(typeof(ConfiguracionPublicacionResponse), StatusCodes.Status200OK)]
	public ActionResult<ConfiguracionPublicacionResponse> ObtenerDePublicacion()
		=> Ok(_servicio.ObtenerDePublicacion());
}

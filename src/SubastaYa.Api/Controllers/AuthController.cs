using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.DTOs.Entrada;
using SubastaYa.Application.DTOs.Salida;
using SubastaYa.Application.Servicios;

namespace SubastaYa.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
	private readonly IServicioDeAutenticacion _servicio;

	public AuthController(IServicioDeAutenticacion servicio)
	{
		_servicio = servicio;
	}

	[HttpPost("sessions")]
	[ProducesResponseType(typeof(SesionResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
	public async Task<IActionResult> IniciarSesion([FromBody] LoginRequest solicitud)
	{
		var sesion = await _servicio.IniciarSesionAsync(solicitud);

		return sesion is null
			? Unauthorized(new ErrorResponse("Email o contraseña incorrectos."))
			: Ok(sesion);
	}
}

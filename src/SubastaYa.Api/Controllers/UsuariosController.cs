using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubastaYa.Api.Servicios;
using SubastaYa.Application.Consultas;
using SubastaYa.Application.DTOs.Entrada;
using SubastaYa.Application.DTOs.Salida;
using SubastaYa.Application.Servicios;

namespace SubastaYa.Api.Controllers;

[ApiController]
[Route("api/v1/users")]
public class UsuariosController : ControllerBase
{
	private readonly IServicioDeUsuarios _servicio;
	private readonly IConsultasDeUsuarios _consultas;

	public UsuariosController(IServicioDeUsuarios servicio, IConsultasDeUsuarios consultas)
	{
		_servicio = servicio;
		_consultas = consultas;
	}

	[HttpPost]
	[ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
	public async Task<IActionResult> Registrar([FromBody] RegistroRequest solicitud)
	{
		var usuario = await _servicio.RegistrarAsync(solicitud);
		return StatusCode(StatusCodes.Status201Created, usuario);
	}

	[HttpGet("me")]
	[Authorize]
	[ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
	public async Task<IActionResult> ObtenerActual()
	{
		var id = UsuarioActual.ObtenerId(User);
		return Ok(await _consultas.ObtenerPorIdAsync(id));
	}

	[HttpGet("me/bids")]
	[Authorize]
	[ProducesResponseType(typeof(IReadOnlyList<MiParticipacionResponse>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<IReadOnlyList<MiParticipacionResponse>>> ObtenerParticipaciones()
	{
		var usuarioId = UsuarioActual.ObtenerId(User);
		return Ok(await _consultas.ObtenerParticipacionesAsync(usuarioId));
	}
}

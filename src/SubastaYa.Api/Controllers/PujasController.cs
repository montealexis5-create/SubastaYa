using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubastaYa.Api.Servicios;
using SubastaYa.Application.Consultas;
using SubastaYa.Application.DTOs.Entrada;
using SubastaYa.Application.DTOs.Salida;
using SubastaYa.Application.Servicios;

namespace SubastaYa.Api.Controllers;

[ApiController]
[Route("api/v1/auctions/{subastaId:int}/bids")]
public class PujasController : ControllerBase
{
	private readonly IServicioDePujas _servicio;
	private readonly IConsultasDePujas _consultas;

	public PujasController(IServicioDePujas servicio, IConsultasDePujas consultas)
	{
		_servicio = servicio;
		_consultas = consultas;
	}

	[Authorize]
	[HttpPost]
	[ProducesResponseType(typeof(PujaRegistradaResponse), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
	[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(ConflictoPujaResponse), StatusCodes.Status409Conflict)]
	[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
	public async Task<ActionResult<PujaRegistradaResponse>> Crear(
		int subastaId, CrearPujaRequest request)
	{
		var usuarioId = UsuarioActual.ObtenerId(User);
		var puja = await _servicio.RegistrarPujaAsync(subastaId, usuarioId, request.Monto);

		return CreatedAtAction(nameof(ObtenerHistorial), new { subastaId }, puja);
	}

	[HttpGet]
	[ProducesResponseType(typeof(IReadOnlyList<PujaResponse>), StatusCodes.Status200OK)]
	[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
	public async Task<ActionResult<IReadOnlyList<PujaResponse>>> ObtenerHistorial(int subastaId)
		=> Ok(await _consultas.ObtenerHistorialAsync(subastaId));
}

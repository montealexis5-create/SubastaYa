using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubastaYa.Api.Servicios;
using SubastaYa.Application.Consultas;
using SubastaYa.Application.DTOs.Entrada;
using SubastaYa.Application.DTOs.Salida;
using SubastaYa.Application.Servicios;

namespace SubastaYa.Api.Controllers;

[ApiController]
[Route("api/v1/wallets")]
[Authorize]
public class BilleterasController : ControllerBase
{
	private readonly IServicioDeBilleteras _servicio;
	private readonly IConsultasDeBilleteras _consultas;

	public BilleterasController(IServicioDeBilleteras servicio, IConsultasDeBilleteras consultas)
	{
		_servicio = servicio;
		_consultas = consultas;
	}

	[HttpGet("me")]
	[ProducesResponseType(typeof(BilleteraResponse), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
	public async Task<ActionResult<BilleteraResponse>> ObtenerActual()
	{
		var usuarioId = UsuarioActual.ObtenerId(User);
		return Ok(await _consultas.ObtenerPorUsuarioAsync(usuarioId));
	}

	[HttpPost("me/deposits")]
	[ProducesResponseType(typeof(BilleteraResponse), StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
	[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
	public async Task<ActionResult<BilleteraResponse>> Depositar([FromBody] DepositoRequest solicitud)
	{
		var usuarioId = UsuarioActual.ObtenerId(User);
		var billetera = await _servicio.DepositarAsync(usuarioId, solicitud.Monto);

		return CreatedAtAction(nameof(ObtenerActual), billetera);
	}

	[HttpGet("me/entries")]
	[ProducesResponseType(typeof(PaginaResponse<MovimientoResponse>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
	public async Task<ActionResult<PaginaResponse<MovimientoResponse>>> ObtenerMovimientos(
		[FromQuery] PaginacionRequest paginacion)
	{
		var usuarioId = UsuarioActual.ObtenerId(User);
		return Ok(await _consultas.ObtenerMovimientosAsync(usuarioId, paginacion));
	}
}

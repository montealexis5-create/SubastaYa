using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubastaYa.Api.Servicios;
using SubastaYa.Application.Consultas;
using SubastaYa.Application.DTOs.Entrada;
using SubastaYa.Application.DTOs.Salida;
using SubastaYa.Application.Servicios;

namespace SubastaYa.Api.Controllers;

[ApiController]
[Route("api/v1/auctions")]
public class SubastasController : ControllerBase
{
	private readonly IServicioDeSubastas _servicio;
	private readonly IConsultasDeSubastas _consultas;

	public SubastasController(IServicioDeSubastas servicio, IConsultasDeSubastas consultas)
	{
		_servicio = servicio;
		_consultas = consultas;
	}

	[HttpGet]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<PaginaResponse<SubastaTarjetaResponse>>> ObtenerCatalogo(
		[FromQuery] FiltroSubastasRequest filtro)
		=> Ok(await _consultas.ObtenerCatalogoAsync(filtro));

	[HttpGet("{id:int}")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status404NotFound)]
	public async Task<ActionResult<SubastaDetalleResponse>> ObtenerPorId(int id)
	{
		var respuesta = await _consultas.ObtenerDetalleAsync(id);
		return Ok(respuesta);
	}

	[Authorize]
	[HttpPost]
	[ProducesResponseType(StatusCodes.Status201Created)]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	[ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
	public async Task<ActionResult<SubastaDetalleResponse>> Publicar(PublicarSubastaRequest request)
	{
		var vendedorId = UsuarioActual.ObtenerId(User);
		var subasta = await _servicio.PublicarAsync(request, vendedorId);

		return CreatedAtAction(nameof(ObtenerPorId), new { id = subasta.Id }, subasta);
	}

	[Authorize]
	[HttpGet("~/api/v1/users/me/auctions")]
	[ProducesResponseType(typeof(IReadOnlyList<MiPublicacionResponse>), StatusCodes.Status200OK)]
	[ProducesResponseType(StatusCodes.Status401Unauthorized)]
	public async Task<ActionResult<IReadOnlyList<MiPublicacionResponse>>> ObtenerMisPublicaciones()
	{
		var vendedorId = UsuarioActual.ObtenerId(User);
		return Ok(await _consultas.ObtenerPublicacionesAsync(vendedorId));
	}
}

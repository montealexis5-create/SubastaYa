using Microsoft.AspNetCore.Mvc;
using SubastaYa.Application.Consultas;
using SubastaYa.Application.DTOs.Salida;

namespace SubastaYa.Api.Controllers;

[ApiController]
[Route("api/v1/categories")]
public class CategoriasController : ControllerBase
{
	private readonly IConsultasDeCategorias _consultas;

	public CategoriasController(IConsultasDeCategorias consultas)
	{
		_consultas = consultas;
	}

	[HttpGet]
	[ProducesResponseType(typeof(IEnumerable<CategoriaResponse>), StatusCodes.Status200OK)]
	public async Task<ActionResult<IEnumerable<CategoriaResponse>>> ObtenerCategorias()
		=> Ok(await _consultas.ObtenerTodasAsync());
}

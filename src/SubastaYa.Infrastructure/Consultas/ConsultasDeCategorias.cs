using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.Consultas;
using SubastaYa.Application.DTOs.Salida;
using SubastaYa.Infrastructure.Persistencia;

namespace SubastaYa.Infrastructure.Consultas;

public class ConsultasDeCategorias : IConsultasDeCategorias
{
	private readonly AppDbContext _contexto;

	public ConsultasDeCategorias(AppDbContext contexto)
	{
		_contexto = contexto;
	}

	public async Task<IEnumerable<CategoriaResponse>> ObtenerTodasAsync()
	{
		return await _contexto.Categorias
			.AsNoTracking()
			.OrderBy(c => c.Nombre)
			.Select(c => new CategoriaResponse
			{
				Id = c.Id,
				Nombre = c.Nombre,
				UrlIcono = c.UrlIcono
			})
			.ToListAsync();
	}
}

using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.Consultas;
using SubastaYa.Application.DTOs.Entrada;
using SubastaYa.Application.DTOs.Salida;
using SubastaYa.Application.Servicios;
using SubastaYa.Domain.Entidades;
using SubastaYa.Domain.Enums;
using SubastaYa.Domain.Excepciones;
using SubastaYa.Infrastructure.Persistencia;

namespace SubastaYa.Infrastructure.Consultas;

public class ConsultasDeSubastas : IConsultasDeSubastas
{
	private const string OrdenPorPuja = "puja";
	private const string OrdenPorReciente = "reciente";

	private readonly AppDbContext _contexto;
	private readonly IReloj _reloj;

	public ConsultasDeSubastas(AppDbContext contexto, IReloj reloj)
	{
		_contexto = contexto;
		_reloj = reloj;
	}

	public async Task<PaginaResponse<SubastaTarjetaResponse>> ObtenerCatalogoAsync(FiltroSubastasRequest filtro)
	{
		var ahora = _reloj.AhoraUtc;
		var consulta = Ordenar(Filtrar(_contexto.Subastas.AsNoTracking(), filtro), filtro.Orden, ahora);

		var totalElementos = await consulta.CountAsync();

		var items = await consulta
			.Skip((filtro.Pagina - 1) * filtro.Tamano)
			.Take(filtro.Tamano)
			.Select(s => new SubastaTarjetaResponse
			{
				Id = s.Id,
				Titulo = s.Titulo,
				UrlImagen = s.UrlImagen,
				NombreCategoria = s.Categoria.Nombre,
				PujaActual = s.PujaActual,
				CantidadOfertas = s.Pujas.Count,
				FechaInicio = s.FechaInicio,
				FechaFin = s.FechaFin,
				Estado = s.Estado.ToString(),
				FechaServidor = ahora
			})
			.ToListAsync();

		return PaginaResponse.Crear(items, filtro.Pagina, filtro.Tamano, totalElementos);
	}

	public async Task<SubastaDetalleResponse> ObtenerDetalleAsync(int id)
	{
		var ahora = _reloj.AhoraUtc;

		return await _contexto.Subastas
			.AsNoTracking()
			.Where(s => s.Id == id)
			.Select(s => new SubastaDetalleResponse
			{
				Id = s.Id,
				Titulo = s.Titulo,
				Descripcion = s.Descripcion,
				UrlImagen = s.UrlImagen,
				NombreCategoria = s.Categoria.Nombre,
				PrecioBase = s.PrecioBase,
				IncrementoMinimo = s.IncrementoMinimo,
				PujaActual = s.PujaActual,
				CantidadOfertas = s.Pujas.Count,
				FechaInicio = s.FechaInicio,
				FechaFin = s.FechaFin,
				Estado = s.Estado.ToString(),
				SeudonimoVendedor = s.Vendedor.Seudonimo,
				SeudonimoLider = (s.Lider == null) ? null : s.Lider.Seudonimo,
				FechaServidor = ahora
			})
			.FirstOrDefaultAsync()
			?? throw new RecursoNoEncontradoException("La subasta no existe.");
	}

	public async Task<IReadOnlyList<MiPublicacionResponse>> ObtenerPublicacionesAsync(int vendedorId)
	{
		var publicaciones = await _contexto.Subastas
			.AsNoTracking()
			.Where(s => s.VendedorId == vendedorId)
			.OrderByDescending(s => s.FechaCreacion)
			.Select(s => new
			{
				s.Id,
				s.Titulo,
				s.UrlImagen,
				s.Estado,
				s.PujaActual,
				CantidadOfertas = s.Pujas.Count,
				s.FechaFin,
				SeudonimoLider = (s.Lider == null) ? null : s.Lider.Seudonimo
			})
			.ToListAsync();

		return publicaciones
			.Select(p => new MiPublicacionResponse
			{
				Id = p.Id,
				Titulo = p.Titulo,
				UrlImagen = p.UrlImagen,
				Estado = p.Estado.ToString(),
				PujaActual = p.PujaActual,
				CantidadOfertas = p.CantidadOfertas,
				FechaFin = p.FechaFin,
				Recaudado = ResultadosDeSubasta.Recaudado(p.Estado, p.PujaActual),
				EstadoAdjudicacion = ResultadosDeSubasta.ParaVendedor(p.Estado),
				SeudonimoGanador = ResultadosDeSubasta.Ganador(p.Estado, p.SeudonimoLider)
			})
			.ToList();
	}

	private static IQueryable<Subasta> Filtrar(IQueryable<Subasta> consulta, FiltroSubastasRequest filtro)
	{
		if (filtro.Estado.HasValue)
		{
			consulta = consulta.Where(s => s.Estado == filtro.Estado.Value);
		}

		if (filtro.CategoriaId.HasValue)
		{
			consulta = consulta.Where(s => s.CategoriaId == filtro.CategoriaId.Value);
		}

		if (filtro.PrecioMin.HasValue)
		{
			consulta = consulta.Where(s => s.PujaActual >= filtro.PrecioMin.Value);
		}

		if (filtro.PrecioMax.HasValue)
		{
			consulta = consulta.Where(s => s.PujaActual <= filtro.PrecioMax.Value);
		}

		if (!string.IsNullOrWhiteSpace(filtro.Busqueda))
		{
			consulta = consulta.Where(s => EF.Functions.ILike(s.Titulo, $"%{filtro.Busqueda.Trim()}%"));
		}

		return consulta;
	}

	private static IQueryable<Subasta> Ordenar(IQueryable<Subasta> consulta, string? orden, DateTime ahora)
	{
		return orden switch
		{
			OrdenPorPuja => consulta.OrderByDescending(s => s.PujaActual),
			OrdenPorReciente => consulta.OrderByDescending(s => s.FechaCreacion),
			_ => consulta.OrderBy(s => s.FechaFin < ahora).ThenBy(s => s.FechaFin)
		};
	}
}

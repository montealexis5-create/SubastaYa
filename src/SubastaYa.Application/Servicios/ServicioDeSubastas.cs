using SubastaYa.Application.Consultas;
using SubastaYa.Application.DTOs.Entrada;
using SubastaYa.Application.DTOs.Salida;
using SubastaYa.Domain.Entidades;
using SubastaYa.Domain.Enums;
using SubastaYa.Domain.Excepciones;
using SubastaYa.Domain.Persistencia;
using SubastaYa.Domain.Repositorios;

namespace SubastaYa.Application.Servicios;

public class ServicioDeSubastas : IServicioDeSubastas
{
	private const int DiasDeDuracionMaxima = 365;
	private const int MinutosDeToleranciaInicio = 5;

	private static readonly TimeSpan DuracionMaxima = TimeSpan.FromDays(DiasDeDuracionMaxima);
	private static readonly TimeSpan ToleranciaInicioPasado = TimeSpan.FromMinutes(MinutosDeToleranciaInicio);

	private readonly ISubastaRepository _subastas;
	private readonly ICategoriaRepository _categorias;
	private readonly IConsultasDeSubastas _consultas;
	private readonly IUnitOfWork _unidad;

	public ServicioDeSubastas(
		ISubastaRepository subastas,
		ICategoriaRepository categorias,
		IConsultasDeSubastas consultas,
		IUnitOfWork unidad)
	{
		_subastas = subastas;
		_categorias = categorias;
		_consultas = consultas;
		_unidad = unidad;
	}

	public async Task<SubastaDetalleResponse> PublicarAsync(PublicarSubastaRequest request, int vendedorId)
	{
		if (!await _categorias.ExisteAsync(request.CategoriaId))
		{
			throw new ReglaDeNegocioException($"No existe una categoría con id {request.CategoriaId}.");
		}

		var ahora = DateTime.UtcNow;
		var fechaInicio = AUtc(request.FechaInicio!.Value);
		var fechaFin = AUtc(request.FechaFin!.Value);

		ValidarFechas(fechaInicio, fechaFin, ahora);
		ValidarIncremento(request);

		var subasta = Crear(request, vendedorId, fechaInicio, fechaFin, ahora);
		_subastas.Agregar(subasta);
		await _unidad.SaveChangesAsync();

		return await _consultas.ObtenerDetalleAsync(subasta.Id);
	}

	private static void ValidarFechas(DateTime fechaInicio, DateTime fechaFin, DateTime ahora)
	{
		if (fechaInicio < ahora - ToleranciaInicioPasado)
		{
			throw new ReglaDeNegocioException("La fecha de inicio no puede estar en el pasado.");
		}

		if (fechaFin <= fechaInicio)
		{
			throw new ReglaDeNegocioException("La fecha de fin debe ser posterior a la de inicio.");
		}

		if (fechaFin <= ahora)
		{
			throw new ReglaDeNegocioException("La fecha de fin debe ser posterior al momento actual.");
		}

		var limite = ahora + DuracionMaxima;
		if (fechaFin > limite)
		{
			throw new ReglaDeNegocioException($"La subasta no puede terminar más allá del {limite:dd/MM/yyyy}.");
		}
	}

	private static void ValidarIncremento(PublicarSubastaRequest request)
	{
		if (request.IncrementoMinimo > request.PrecioBase)
		{
			throw new ReglaDeNegocioException("El incremento mínimo no puede superar al precio base.");
		}
	}

	private static Subasta Crear(
		PublicarSubastaRequest request, int vendedorId, DateTime fechaInicio, DateTime fechaFin, DateTime ahora)
	{
		return new Subasta
		{
			VendedorId = vendedorId,
			CategoriaId = request.CategoriaId,
			Titulo = request.Titulo,
			Descripcion = request.Descripcion,
			UrlImagen = request.UrlImagen,
			PrecioBase = request.PrecioBase,
			IncrementoMinimo = request.IncrementoMinimo,
			PujaActual = request.PrecioBase,
			LiderId = null,
			FechaInicio = fechaInicio,
			FechaFin = fechaFin,
			Estado = (fechaInicio <= ahora) ? EstadoSubasta.Activa : EstadoSubasta.Programada,
			Version = 0,
			FechaCreacion = ahora
		};
	}

	private static DateTime AUtc(DateTime fecha) => fecha.Kind switch
	{
		DateTimeKind.Utc => fecha,
		DateTimeKind.Local => fecha.ToUniversalTime(),
		_ => DateTime.SpecifyKind(fecha, DateTimeKind.Utc)
	};
}

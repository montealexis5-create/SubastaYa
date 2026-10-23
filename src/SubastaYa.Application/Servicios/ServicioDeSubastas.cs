using Microsoft.Extensions.Options;
using SubastaYa.Application.Configuracion;
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
	private readonly TimeSpan _duracionMaxima;
	private readonly TimeSpan _toleranciaInicioPasado;

	private readonly ISubastaRepository _subastas;
	private readonly ICategoriaRepository _categorias;
	private readonly IConsultasDeSubastas _consultas;
	private readonly IUnitOfWork _unidad;

	public ServicioDeSubastas(
		ISubastaRepository subastas,
		ICategoriaRepository categorias,
		IConsultasDeSubastas consultas,
		IUnitOfWork unidad,
		IOptions<OpcionesPublicacion> opciones)
	{
		_subastas = subastas;
		_categorias = categorias;
		_consultas = consultas;
		_unidad = unidad;
		_duracionMaxima = TimeSpan.FromDays(opciones.Value.DuracionMaximaDias);
		_toleranciaInicioPasado = TimeSpan.FromMinutes(opciones.Value.ToleranciaInicioMinutos);
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

	private void ValidarFechas(DateTime fechaInicio, DateTime fechaFin, DateTime ahora)
	{
		if (fechaInicio < ahora - _toleranciaInicioPasado)
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

		var limite = ahora + _duracionMaxima;
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

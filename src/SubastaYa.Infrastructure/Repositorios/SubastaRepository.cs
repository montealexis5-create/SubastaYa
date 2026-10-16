using Microsoft.EntityFrameworkCore;
using SubastaYa.Domain.Entidades;
using SubastaYa.Domain.Enums;
using SubastaYa.Domain.Repositorios;
using SubastaYa.Infrastructure.Persistencia;

namespace SubastaYa.Infrastructure.Repositorios;

public class SubastaRepository : ISubastaRepository
{
	private readonly AppDbContext _contexto;

	public SubastaRepository(AppDbContext contexto)
	{
		_contexto = contexto;
	}

	public Task<Subasta?> ObtenerPorIdAsync(int id) => _contexto.Subastas.FirstOrDefaultAsync(s => s.Id == id);

	public async Task<EstadoDeOferta> ObtenerEstadoDeOfertaAsync(int id)
	{
		var actual = await _contexto.Subastas
			.AsNoTracking()
			.Where(s => s.Id == id)
			.Select(s => new { s.PujaActual, s.IncrementoMinimo, s.FechaFin })
			.FirstAsync();

		return new EstadoDeOferta(actual.PujaActual, actual.IncrementoMinimo, actual.FechaFin);
	}

	public async Task<IReadOnlyList<int>> ObtenerIdsProgramadasParaActivarAsync(
		DateTime ahora, CancellationToken cancelacion)
	{
		return await _contexto.Subastas
			.AsNoTracking()
			.Where(s => s.Estado == EstadoSubasta.Programada && s.FechaInicio <= ahora)
			.Select(s => s.Id)
			.ToListAsync(cancelacion);
	}

	public async Task<IReadOnlyList<int>> ObtenerIdsActivasVencidasAsync(
		DateTime ahora, CancellationToken cancelacion)
	{
		return await _contexto.Subastas
			.AsNoTracking()
			.Where(s => s.Estado == EstadoSubasta.Activa && s.FechaFin <= ahora)
			.Select(s => s.Id)
			.ToListAsync(cancelacion);
	}

	public Task<Subasta?> ObtenerProgramadaParaActivarAsync(int id, DateTime ahora, CancellationToken cancelacion)
	{
		return _contexto.Subastas.FirstOrDefaultAsync(
			s => s.Id == id && s.Estado == EstadoSubasta.Programada && s.FechaInicio <= ahora,
			cancelacion);
	}

	public Task<Subasta?> ObtenerActivaVencidaAsync(int id, DateTime ahora, CancellationToken cancelacion)
	{
		return _contexto.Subastas.FirstOrDefaultAsync(
			s => s.Id == id && s.Estado == EstadoSubasta.Activa && s.FechaFin <= ahora,
			cancelacion);
	}

	public void Agregar(Subasta subasta) => _contexto.Subastas.Add(subasta);
}

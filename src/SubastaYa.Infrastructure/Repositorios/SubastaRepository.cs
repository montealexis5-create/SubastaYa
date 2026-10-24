using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Domain.Entidades;
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

	public async Task<IReadOnlyList<int>> ObtenerIdsAsync(
		Expression<Func<Subasta, bool>> criterio, CancellationToken cancelacion)
	{
		return await _contexto.Subastas
			.AsNoTracking()
			.Where(criterio)
			.Select(s => s.Id)
			.ToListAsync(cancelacion);
	}

	public Task<Subasta?> ObtenerAsync(
		int id, Expression<Func<Subasta, bool>> criterio, CancellationToken cancelacion)
	{
		return _contexto.Subastas
			.Where(s => s.Id == id)
			.Where(criterio)
			.FirstOrDefaultAsync(cancelacion);
	}

	public void Agregar(Subasta subasta) => _contexto.Subastas.Add(subasta);
}

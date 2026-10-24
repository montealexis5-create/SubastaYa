using System.Linq.Expressions;
using SubastaYa.Domain.Entidades;

namespace SubastaYa.Domain.Repositorios;

public interface ISubastaRepository
{
	Task<Subasta?> ObtenerPorIdAsync(int id);

	Task<IReadOnlyList<int>> ObtenerIdsAsync(
		Expression<Func<Subasta, bool>> criterio, CancellationToken cancelacion);

	Task<Subasta?> ObtenerAsync(
		int id, Expression<Func<Subasta, bool>> criterio, CancellationToken cancelacion);

	void Agregar(Subasta subasta);
}

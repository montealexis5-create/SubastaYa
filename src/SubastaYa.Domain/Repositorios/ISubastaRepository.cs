using SubastaYa.Domain.Entidades;

namespace SubastaYa.Domain.Repositorios;

public interface ISubastaRepository
{
	Task<Subasta?> ObtenerPorIdAsync(int id);

	Task<EstadoDeOferta> ObtenerEstadoDeOfertaAsync(int id);

	Task<IReadOnlyList<int>> ObtenerIdsProgramadasParaActivarAsync(
		DateTime ahora, CancellationToken cancelacion);

	Task<IReadOnlyList<int>> ObtenerIdsActivasVencidasAsync(
		DateTime ahora, CancellationToken cancelacion);

	Task<Subasta?> ObtenerProgramadaParaActivarAsync(
		int id, DateTime ahora, CancellationToken cancelacion);

	Task<Subasta?> ObtenerActivaVencidaAsync(
		int id, DateTime ahora, CancellationToken cancelacion);

	void Agregar(Subasta subasta);
}

using Microsoft.Extensions.Logging;
using SubastaYa.Domain.Excepciones;
using SubastaYa.Domain.Persistencia;

namespace SubastaYa.Application.Servicios;

public class EjecucionTransaccional : IEjecucionTransaccional
{
	private readonly IUnitOfWork _unidad;
	private readonly ILogger<EjecucionTransaccional> _logger;

	public EjecucionTransaccional(IUnitOfWork unidad, ILogger<EjecucionTransaccional> logger)
	{
		_unidad = unidad;
		_logger = logger;
	}

	public async Task<bool> EjecutarAsync(
		int subastaId, string operacion, Func<Task> trabajo, CancellationToken cancelacion)
	{
		await using var transaccion = await _unidad.BeginTransactionAsync(cancelacion);

		try
		{
			await trabajo();
			await transaccion.CommitAsync(cancelacion);
			return true;
		}
		catch (ActualizacionConcurrenteException)
		{
			await transaccion.RollbackAsync(cancelacion);
			_unidad.ClearTracking();
			_logger.LogWarning(
				"No se pudo {Operacion} la subasta {SubastaId}: cambió mientras se procesaba. "
				+ "Se reintenta en el próximo ciclo.",
				operacion, subastaId);
			return false;
		}
	}
}

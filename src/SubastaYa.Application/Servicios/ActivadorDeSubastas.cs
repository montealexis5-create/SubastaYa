using Microsoft.Extensions.Logging;
using SubastaYa.Domain;
using SubastaYa.Domain.Persistencia;
using SubastaYa.Domain.Repositorios;

namespace SubastaYa.Application.Servicios;

public class ActivadorDeSubastas : IActivadorDeSubastas
{
	private const string Operacion = "activar";

	private readonly ISubastaRepository _subastas;
	private readonly IServicioDeAuditoria _auditoria;
	private readonly IUnitOfWork _unidad;
	private readonly IEjecucionTransaccional _transaccion;
	private readonly INotificadorDeSubastas _notificador;
	private readonly ILogger<ActivadorDeSubastas> _logger;

	public ActivadorDeSubastas(
		ISubastaRepository subastas,
		IServicioDeAuditoria auditoria,
		IUnitOfWork unidad,
		IEjecucionTransaccional transaccion,
		INotificadorDeSubastas notificador,
		ILogger<ActivadorDeSubastas> logger)
	{
		_subastas = subastas;
		_auditoria = auditoria;
		_unidad = unidad;
		_transaccion = transaccion;
		_notificador = notificador;
		_logger = logger;
	}

	/// Programada a Activa. Se audita igual que el cierre, el enunciado pide todos los cambios de estado.
	public async Task ActivarAsync(int subastaId, DateTime ahora, CancellationToken cancelacion)
	{
		var subasta = await _subastas.ObtenerProgramadaParaActivarAsync(subastaId, ahora, cancelacion);
		if (subasta is null)
		{
			return;
		}

		var activada = await _transaccion.EjecutarAsync(subasta.Id, Operacion, async () =>
		{
			subasta.Activar();
			await _unidad.SaveChangesAsync(cancelacion);

			await _auditoria.RegistrarAsync(
				EntidadesAuditables.Subasta, subasta.Id, AccionesAuditoria.ActivacionWorker,
				null, new { fechaInicio = subasta.FechaInicio, fechaFin = subasta.FechaFin });
		}, cancelacion);

		if (!activada)
		{
			return;
		}

		_logger.LogInformation("Subasta {SubastaId} activada.", subasta.Id);

		await _notificador.SubastaActivadaAsync(subasta.Id, cancelacion);
	}
}

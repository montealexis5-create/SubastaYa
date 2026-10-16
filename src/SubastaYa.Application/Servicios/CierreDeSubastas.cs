using Microsoft.Extensions.Logging;
using SubastaYa.Domain;
using SubastaYa.Domain.Entidades;
using SubastaYa.Domain.Persistencia;
using SubastaYa.Domain.Repositorios;

namespace SubastaYa.Application.Servicios;

public class CierreDeSubastas : ICierreDeSubastas
{
	private const string OperacionCerrar = "cerrar";
	private const string OperacionDeclararDesierta = "declarar desierta";

	private readonly ISubastaRepository _subastas;
	private readonly IMovimientosDeSaldo _saldos;
	private readonly IServicioDeAuditoria _auditoria;
	private readonly IUnitOfWork _unidad;
	private readonly IEjecucionTransaccional _transaccion;
	private readonly INotificadorDeSubastas _notificador;
	private readonly ILogger<CierreDeSubastas> _logger;

	public CierreDeSubastas(
		ISubastaRepository subastas,
		IMovimientosDeSaldo saldos,
		IServicioDeAuditoria auditoria,
		IUnitOfWork unidad,
		IEjecucionTransaccional transaccion,
		INotificadorDeSubastas notificador,
		ILogger<CierreDeSubastas> logger)
	{
		_subastas = subastas;
		_saldos = saldos;
		_auditoria = auditoria;
		_unidad = unidad;
		_transaccion = transaccion;
		_notificador = notificador;
		_logger = logger;
	}

	public async Task CerrarAsync(int subastaId, DateTime ahora, CancellationToken cancelacion)
	{
		var subasta = await _subastas.ObtenerActivaVencidaAsync(subastaId, ahora, cancelacion);
		if (subasta is null)
		{
			return;
		}

		if (subasta.LiderId is null)
		{
			await CerrarDesiertaAsync(subasta, cancelacion);
		}
		else
		{
			await CerrarConGanadorAsync(subasta, ahora, cancelacion);
		}
	}

	/// Paga al vendedor con lo que el comprador tenia retenido.
	/// El cambio de estado va en la misma transaccion para no pagar dos veces.
	private async Task CerrarConGanadorAsync(Subasta subasta, DateTime ahora, CancellationToken cancelacion)
	{
		var compradorId = subasta.LiderId!.Value;
		var vendedorId = subasta.VendedorId;
		var monto = subasta.PujaActual;

		var cerrada = await _transaccion.EjecutarAsync(subasta.Id, OperacionCerrar, async () =>
		{
			await _saldos.PagarAsync(compradorId, monto, subasta.Id, ahora, cancelacion);
			await _saldos.CobrarAsync(vendedorId, monto, subasta.Id, ahora, cancelacion);

			subasta.Finalizar();
			await _unidad.SaveChangesAsync(cancelacion);

			await _auditoria.RegistrarAsync(
				EntidadesAuditables.Subasta, subasta.Id, AccionesAuditoria.CierreWorker,
				null, new { compradorId, vendedorId, monto });
		}, cancelacion);

		if (!cerrada)
		{
			return;
		}

		_logger.LogInformation(
			"Subasta {SubastaId} finalizada: {Monto} del usuario {CompradorId} al usuario {VendedorId}.",
			subasta.Id, monto, compradorId, vendedorId);

		await _notificador.SubastaFinalizadaAsync(subasta.Id, cancelacion);
	}

	private async Task CerrarDesiertaAsync(Subasta subasta, CancellationToken cancelacion)
	{
		var cerrada = await _transaccion.EjecutarAsync(subasta.Id, OperacionDeclararDesierta, async () =>
		{
			subasta.DeclararDesierta();
			await _unidad.SaveChangesAsync(cancelacion);

			await _auditoria.RegistrarAsync(
				EntidadesAuditables.Subasta, subasta.Id, AccionesAuditoria.CierreDesierta,
				null, new { precioBase = subasta.PrecioBase, fechaFin = subasta.FechaFin });
		}, cancelacion);

		if (!cerrada)
		{
			return;
		}

		_logger.LogInformation("Subasta {SubastaId} declarada desierta.", subasta.Id);

		await _notificador.SubastaFinalizadaAsync(subasta.Id, cancelacion);
	}
}

using Microsoft.Extensions.Options;
using SubastaYa.Application.Configuracion;
using SubastaYa.Application.DTOs.Salida;
using SubastaYa.Domain;
using SubastaYa.Domain.Entidades;
using SubastaYa.Domain.Excepciones;
using SubastaYa.Domain.Persistencia;
using SubastaYa.Domain.Repositorios;

namespace SubastaYa.Application.Servicios;

public class RegistradorDePujas : IRegistradorDePujas
{
	private const string MensajeDeConflicto = "Otra oferta se registró antes que la tuya.";

	private readonly IMovimientosDeSaldo _saldos;
	private readonly IPujaRepository _pujas;
	private readonly ISubastaRepository _subastas;
	private readonly IServicioDeAuditoria _auditoria;
	private readonly IUnitOfWork _unidad;
	private readonly OpcionesAntiSniping _antiSniping;

	public RegistradorDePujas(
		IMovimientosDeSaldo saldos,
		IPujaRepository pujas,
		ISubastaRepository subastas,
		IServicioDeAuditoria auditoria,
		IUnitOfWork unidad,
		IOptions<OpcionesAntiSniping> antiSniping)
	{
		_saldos = saldos;
		_pujas = pujas;
		_subastas = subastas;
		_auditoria = auditoria;
		_unidad = unidad;
		_antiSniping = antiSniping.Value;
	}

	/// Todo en una sola transaccion: libera al lider anterior, retiene al nuevo y guarda la puja.
	public async Task<ResultadoDeOferta> RegistrarAsync(
		Subasta subasta, Billetera billetera, int usuarioId, decimal monto)
	{
		await using var transaccion = await _unidad.BeginTransactionAsync();

		try
		{
			var resultado = await AplicarAsync(subasta, billetera, usuarioId, monto);
			await transaccion.CommitAsync();
			return resultado;
		}
		catch (Exception ex) when (ex is ActualizacionConcurrenteException or SaldoRetenidoNegativoException)
		{
			throw await ResolverConflictoAsync(transaccion, subasta.Id, usuarioId, monto);
		}
	}

	private async Task<ResultadoDeOferta> AplicarAsync(
		Subasta subasta, Billetera billetera, int usuarioId, decimal monto)
	{
		var ahora = DateTime.UtcNow;
		var fechaFinAnterior = subasta.FechaFin;

		if (subasta.LiderId is int liderAnteriorId)
		{
			await _saldos.LiberarRetencionAsync(liderAnteriorId, subasta.PujaActual, subasta.Id, ahora);
		}

		await _saldos.RetenerAsync(billetera, monto, subasta.Id, ahora);

		var puja = new Puja
		{
			SubastaId = subasta.Id,
			CompradorId = usuarioId,
			Monto = monto,
			FechaPuja = ahora
		};
		_pujas.Agregar(puja);

		var tiempoExtendido = subasta.RecibirOferta(
			usuarioId, monto, ahora, _antiSniping.UmbralSegundos, _antiSniping.ExtensionMinutos);

		await _unidad.SaveChangesAsync();

		if (tiempoExtendido)
		{
			await _auditoria.RegistrarAsync(
				EntidadesAuditables.Subasta, subasta.Id, AccionesAuditoria.ExtensionTiempo,
				usuarioId, new { fechaAnterior = fechaFinAnterior, fechaNueva = subasta.FechaFin });
		}

		return new ResultadoDeOferta(puja, tiempoExtendido);
	}

	/// Deshace la transaccion y deja el rechazo en auditoria.
	/// La auditoria va despues del rollback, si no se borraria con el.
	private async Task<ConflictoDeConcurrenciaException> ResolverConflictoAsync(
		ITransaction transaccion, int subastaId, int usuarioId, decimal monto)
	{
		await transaccion.RollbackAsync();
		_unidad.ClearTracking();

		var actual = await _subastas.ObtenerEstadoDeOfertaAsync(subastaId);

		await _auditoria.RegistrarAsync(
			EntidadesAuditables.Subasta, subastaId, AccionesAuditoria.PujaRechazadaConcurrencia,
			usuarioId, new { montoOfrecido = monto, pujaActual = actual.PujaActual });

		var minimo = FormatoDePesos.Formatear(actual.MontoMinimo);

		return new ConflictoDeConcurrenciaException(
			MensajeDeConflicto,
			new ConflictoPujaResponse
			{
				Mensaje = $"{MensajeDeConflicto} La oferta mínima ahora es de {minimo}.",
				PujaActual = actual.PujaActual,
				MontoMinimo = actual.MontoMinimo,
				FechaFin = actual.FechaFin
			});
	}
}

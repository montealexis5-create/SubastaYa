using SubastaYa.Domain;
using SubastaYa.Domain.Entidades;
using SubastaYa.Domain.Enums;
using SubastaYa.Domain.Excepciones;
using SubastaYa.Domain.Repositorios;

namespace SubastaYa.Application.Servicios;

public class ValidadorDePujas : IValidadorDePujas
{
	private readonly ISubastaRepository _subastas;
	private readonly IBilleteraRepository _billeteras;
	private readonly IServicioDeAuditoria _auditoria;

	public ValidadorDePujas(
		ISubastaRepository subastas,
		IBilleteraRepository billeteras,
		IServicioDeAuditoria auditoria)
	{
		_subastas = subastas;
		_billeteras = billeteras;
		_auditoria = auditoria;
	}

	/// Rechaza lo que no puede ser una oferta antes de tocar plata.
	/// El orden de las reglas importa: el 403 va antes del estado de la subasta.
	public async Task<(Subasta Subasta, Billetera Billetera)> ValidarAsync(
		int subastaId, int usuarioId, decimal monto)
	{
		var subasta = await _subastas.ObtenerPorIdAsync(subastaId)
			?? throw new RecursoNoEncontradoException("La subasta no existe.");

		await ValidarSubastaAsync(subasta, usuarioId, monto);

		var billetera = await _billeteras.ObtenerPorUsuarioAsync(usuarioId)
			?? throw new RecursoNoEncontradoException("El usuario no tiene una billetera asociada.");

		await ValidarSaldoAsync(subasta, billetera, usuarioId, monto);

		return (subasta, billetera);
	}

	private async Task ValidarSubastaAsync(Subasta subasta, int usuarioId, decimal monto)
	{
		if (subasta.VendedorId == usuarioId)
		{
			throw await RechazarAsync(
				subasta, usuarioId, monto,
				"El vendedor no puede pujar en su propia subasta.",
				new AccionProhibidaException("No podés pujar en tu propia subasta."));
		}

		if (subasta.Estado != EstadoSubasta.Activa)
		{
			throw await RechazarPorReglaAsync(subasta, usuarioId, monto, "La subasta no está activa.");
		}

		if (DateTime.UtcNow >= subasta.FechaFin)
		{
			throw await RechazarPorReglaAsync(subasta, usuarioId, monto, "La subasta ya venció.");
		}

		if (subasta.LiderId == usuarioId)
		{
			throw await RechazarAsync(
				subasta, usuarioId, monto,
				"El usuario ya es el líder de la subasta.",
				new ReglaDeNegocioException("Ya sos el líder de esta subasta."));
		}

		if (monto < subasta.MontoMinimo)
		{
			throw await RechazarAsync(
				subasta, usuarioId, monto,
				$"El monto no alcanza el minimo de {subasta.MontoMinimo}.",
				new ReglaDeNegocioException(
					$"La oferta debe ser de al menos {FormatoDePesos.Formatear(subasta.MontoMinimo)}."));
		}
	}

	private async Task ValidarSaldoAsync(Subasta subasta, Billetera billetera, int usuarioId, decimal monto)
	{
		if (billetera.SaldoDisponible >= monto)
		{
			return;
		}

		await _auditoria.RegistrarAsync(
			EntidadesAuditables.Subasta, subasta.Id, AccionesAuditoria.PujaRechazadaSaldo,
			usuarioId, new { monto, disponible = billetera.SaldoDisponible });

		throw new ReglaDeNegocioException(
			$"Saldo insuficiente. Disponible: {FormatoDePesos.Formatear(billetera.SaldoDisponible)}.");
	}

	private Task<Exception> RechazarPorReglaAsync(Subasta subasta, int usuarioId, decimal monto, string motivo)
	{
		return RechazarAsync(subasta, usuarioId, monto, motivo, new ReglaDeNegocioException(motivo));
	}

	private async Task<Exception> RechazarAsync(
		Subasta subasta, int usuarioId, decimal monto, string motivo, Exception error)
	{
		await _auditoria.RegistrarAsync(
			EntidadesAuditables.Subasta, subasta.Id, AccionesAuditoria.PujaRechazadaValidacion,
			usuarioId, new { monto, motivo });

		return error;
	}
}

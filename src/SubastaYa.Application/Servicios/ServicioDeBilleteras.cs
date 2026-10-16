using SubastaYa.Application.DTOs.Salida;
using SubastaYa.Domain;
using SubastaYa.Domain.Entidades;
using SubastaYa.Domain.Enums;
using SubastaYa.Domain.Excepciones;
using SubastaYa.Domain.Persistencia;
using SubastaYa.Domain.Repositorios;

namespace SubastaYa.Application.Servicios;

public class ServicioDeBilleteras : IServicioDeBilleteras
{
	private const string SinBilletera = "El usuario no tiene una billetera asociada.";

	private readonly IBilleteraRepository _billeteras;
	private readonly IAsientoLedgerRepository _ledger;
	private readonly IServicioDeAuditoria _auditoria;
	private readonly IUnitOfWork _unidad;

	public ServicioDeBilleteras(
		IBilleteraRepository billeteras,
		IAsientoLedgerRepository ledger,
		IServicioDeAuditoria auditoria,
		IUnitOfWork unidad)
	{
		_billeteras = billeteras;
		_ledger = ledger;
		_auditoria = auditoria;
		_unidad = unidad;
	}

	/// El saldo y su asiento entran juntos o no entra ninguno.
	public async Task<BilleteraResponse> DepositarAsync(int usuarioId, decimal monto)
	{
		await using var transaccion = await _unidad.BeginTransactionAsync();

		var billetera = await _billeteras.ObtenerPorUsuarioAsync(usuarioId)
			?? throw new RecursoNoEncontradoException(SinBilletera);

		var saldoAnterior = billetera.SaldoTotal;
		billetera.Acreditar(monto);

		await _ledger.AgregarAsync(AsientoLedger.Nuevo(
			billetera.Id, TipoAsiento.Deposito, monto, DescripcionesDeAsiento.Deposito, DateTime.UtcNow));

		await GuardarAsync();

		await _auditoria.RegistrarAsync(
			EntidadesAuditables.Billetera,
			billetera.Id,
			AccionesAuditoria.AcreditacionManual,
			usuarioId,
			new { monto, saldoAnterior, saldoNuevo = billetera.SaldoTotal });

		await transaccion.CommitAsync();

		return BilleteraResponse.Desde(billetera);
	}

	private async Task GuardarAsync()
	{
		try
		{
			await _unidad.SaveChangesAsync();
		}
		catch (ActualizacionConcurrenteException)
		{
			throw new ConflictoDeConcurrenciaException(
				"El saldo cambió mientras se procesaba el depósito. Volvé a intentarlo.");
		}
	}
}

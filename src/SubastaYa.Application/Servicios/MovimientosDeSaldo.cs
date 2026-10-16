using SubastaYa.Domain.Entidades;
using SubastaYa.Domain.Enums;
using SubastaYa.Domain.Repositorios;

namespace SubastaYa.Application.Servicios;

public class MovimientosDeSaldo : IMovimientosDeSaldo
{
	private readonly IBilleteraRepository _billeteras;
	private readonly IAsientoLedgerRepository _ledger;

	public MovimientosDeSaldo(IBilleteraRepository billeteras, IAsientoLedgerRepository ledger)
	{
		_billeteras = billeteras;
		_ledger = ledger;
	}

	public async Task RetenerAsync(Billetera billetera, decimal monto, int subastaId, DateTime fecha)
	{
		billetera.Retener(monto);

		await _ledger.AgregarAsync(AsientoLedger.Nuevo(
			billetera.Id, TipoAsiento.Retencion, monto, DescripcionesDeAsiento.Retencion, fecha, subastaId));
	}

	public async Task LiberarRetencionAsync(
		int usuarioId, decimal monto, int subastaId, DateTime fecha, CancellationToken cancelacion = default)
	{
		var billetera = await ObtenerBilleteraAsync(usuarioId, cancelacion);
		billetera.LiberarRetencion(monto);

		await _ledger.AgregarAsync(AsientoLedger.Nuevo(
			billetera.Id, TipoAsiento.Liberacion, monto, DescripcionesDeAsiento.Liberacion, fecha, subastaId));
	}

	public async Task PagarAsync(
		int compradorId, decimal monto, int subastaId, DateTime fecha, CancellationToken cancelacion = default)
	{
		var billetera = await ObtenerBilleteraAsync(compradorId, cancelacion);
		billetera.PagarRetenido(monto);

		await _ledger.AgregarAsync(AsientoLedger.Nuevo(
			billetera.Id, TipoAsiento.Pago, monto, DescripcionesDeAsiento.Pago, fecha, subastaId));
	}

	public async Task CobrarAsync(
		int vendedorId, decimal monto, int subastaId, DateTime fecha, CancellationToken cancelacion = default)
	{
		var billetera = await ObtenerBilleteraAsync(vendedorId, cancelacion);
		billetera.Acreditar(monto);

		await _ledger.AgregarAsync(AsientoLedger.Nuevo(
			billetera.Id, TipoAsiento.Cobro, monto, DescripcionesDeAsiento.Cobro, fecha, subastaId));
	}

	private async Task<Billetera> ObtenerBilleteraAsync(int usuarioId, CancellationToken cancelacion)
	{
		return await _billeteras.ObtenerPorUsuarioAsync(usuarioId, cancelacion)
			?? throw new InvalidOperationException($"El usuario {usuarioId} no tiene una billetera.");
	}
}

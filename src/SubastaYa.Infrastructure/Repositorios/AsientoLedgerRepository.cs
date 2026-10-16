using Microsoft.EntityFrameworkCore;
using SubastaYa.Domain.Entidades;
using SubastaYa.Domain.Enums;
using SubastaYa.Domain.Repositorios;
using SubastaYa.Infrastructure.Persistencia;

namespace SubastaYa.Infrastructure.Repositorios;

public class AsientoLedgerRepository : IAsientoLedgerRepository
{
	private readonly AppDbContext _contexto;

	public AsientoLedgerRepository(AppDbContext contexto)
	{
		_contexto = contexto;
	}

	public Task AgregarAsync(AsientoLedger asiento)
	{
		_contexto.AsientosLedger.Add(asiento);
		return Task.CompletedTask;
	}

	/// <summary>
	/// Saldo total reconstruido desde los movimientos: si no coincide con
	/// <c>Billetera.SaldoTotal</c>, hay un movimiento sin asiento o al revés.
	/// </summary>
	public async Task<decimal> SumarPorBilleteraAsync(int billeteraId)
	{
		// Retencion y Liberacion mueven el saldo retenido, no el total.
		var total = await _contexto.AsientosLedger
			.AsNoTracking()
			.Where(a => a.BilleteraId == billeteraId)
			.SumAsync(a => (decimal?)(
				a.Tipo == TipoAsiento.Deposito || a.Tipo == TipoAsiento.Cobro ? a.Monto
				: a.Tipo == TipoAsiento.Pago ? -a.Monto
				: 0m));

		// SUM sobre cero filas devuelve NULL, no cero.
		return total ?? 0m;
	}
}

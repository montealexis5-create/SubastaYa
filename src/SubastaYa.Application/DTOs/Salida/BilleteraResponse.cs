using SubastaYa.Domain.Entidades;

namespace SubastaYa.Application.DTOs.Salida;

public class BilleteraResponse
{
	public decimal SaldoTotal { get; set; }

	public decimal SaldoRetenido { get; set; }

	public decimal SaldoDisponible { get; set; }

	public static BilleteraResponse Desde(Billetera billetera) => new()
	{
		SaldoTotal = billetera.SaldoTotal,
		SaldoRetenido = billetera.SaldoRetenido,
		SaldoDisponible = billetera.SaldoDisponible
	};
}

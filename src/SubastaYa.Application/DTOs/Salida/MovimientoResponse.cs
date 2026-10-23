using SubastaYa.Domain.Entidades;
using SubastaYa.Domain.Enums;

namespace SubastaYa.Application.DTOs.Salida;

public class MovimientoResponse
{
	public int Id { get; set; }

	public string Tipo { get; set; } = null!;

	/// Como mueve el saldo: suma, resta o reserva. Lo decide el back.
	public string Efecto { get; set; } = null!;

	public decimal Monto { get; set; }

	public string? Descripcion { get; set; }
	public DateTime Fecha { get; set; }

	public int? SubastaId { get; set; }

	public string? TituloSubasta { get; set; }

	public static MovimientoResponse Desde(AsientoLedger asiento) => new()
	{
		Id = asiento.Id,
		Tipo = asiento.Tipo.ToString(),
		Efecto = EfectoDe(asiento.Tipo),
		Monto = asiento.Monto,
		Descripcion = asiento.Descripcion,
		Fecha = asiento.Fecha,
		SubastaId = asiento.SubastaId,
		TituloSubasta = asiento.Subasta?.Titulo
	};

	private static string EfectoDe(TipoAsiento tipo) => tipo switch
	{
		TipoAsiento.Deposito or TipoAsiento.Cobro => "suma",
		TipoAsiento.Pago => "resta",
		TipoAsiento.Retencion or TipoAsiento.Liberacion => "reserva",
		_ => throw new ArgumentOutOfRangeException(nameof(tipo), tipo, "Tipo de asiento sin efecto definido.")
	};
}

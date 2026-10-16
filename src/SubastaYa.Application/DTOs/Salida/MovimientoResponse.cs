using SubastaYa.Domain.Entidades;

namespace SubastaYa.Application.DTOs.Salida;

public class MovimientoResponse
{
	public int Id { get; set; }

	public string Tipo { get; set; } = null!;

	public decimal Monto { get; set; }

	public string? Descripcion { get; set; }
	public DateTime Fecha { get; set; }

	public int? SubastaId { get; set; }

	public string? TituloSubasta { get; set; }

	public static MovimientoResponse Desde(AsientoLedger asiento) => new()
	{
		Id = asiento.Id,
		Tipo = asiento.Tipo.ToString(),
		Monto = asiento.Monto,
		Descripcion = asiento.Descripcion,
		Fecha = asiento.Fecha,
		SubastaId = asiento.SubastaId,
		TituloSubasta = asiento.Subasta?.Titulo
	};
}

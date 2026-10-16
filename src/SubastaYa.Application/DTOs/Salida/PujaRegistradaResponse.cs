namespace SubastaYa.Application.DTOs.Salida;

public class PujaRegistradaResponse : PujaResponse
{
	public DateTime FechaFin { get; set; }

	public decimal MontoMinimo { get; set; }

	public bool TiempoExtendido { get; set; }
}

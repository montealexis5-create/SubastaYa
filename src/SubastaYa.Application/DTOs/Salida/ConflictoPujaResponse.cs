namespace SubastaYa.Application.DTOs.Salida;

public class ConflictoPujaResponse
{
	public string Mensaje { get; set; } = null!;

	public decimal PujaActual { get; set; }

	public decimal MontoMinimo { get; set; }

	public DateTime FechaFin { get; set; }
}

namespace SubastaYa.Application.DTOs.Salida;

public class PujaResponse
{
	public int Id { get; set; }
	public decimal Monto { get; set; }
	public DateTime FechaPuja { get; set; }
	public string Seudonimo { get; set; } = null!;
}

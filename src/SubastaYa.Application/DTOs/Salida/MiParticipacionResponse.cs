namespace SubastaYa.Application.DTOs.Salida;

public class MiParticipacionResponse
{
	public int SubastaId { get; set; }
	public string Titulo { get; set; } = null!;
	public string UrlImagen { get; set; } = null!;
	public string Estado { get; set; } = null!;

	public decimal MiOfertaMaxima { get; set; }

	public decimal PujaActual { get; set; }

	public DateTime FechaFin { get; set; }

	public string Resultado { get; set; } = null!;
}

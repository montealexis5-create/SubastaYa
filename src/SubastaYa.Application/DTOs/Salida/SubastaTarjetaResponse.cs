namespace SubastaYa.Application.DTOs.Salida;

public class SubastaTarjetaResponse
{
	public int Id { get; set; }
	public string Titulo { get; set; } = null!;
	public string UrlImagen { get; set; } = null!;
	public string NombreCategoria { get; set; } = null!;
	public decimal PujaActual { get; set; }
	public int CantidadOfertas { get; set; }
	public DateTime FechaInicio { get; set; }
	public DateTime FechaFin { get; set; }
	public string Estado { get; set; } = null!;

	public DateTime FechaServidor { get; set; }
}

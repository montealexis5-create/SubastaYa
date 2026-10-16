using System.ComponentModel.DataAnnotations;

namespace SubastaYa.Application.DTOs.Entrada;

public class PaginacionRequest
{
	[Range(1, 100000, ErrorMessage = "La página debe estar entre 1 y 100000.")]
	public int Pagina { get; set; } = 1;

	[Range(1, 100, ErrorMessage = "El tamaño de página debe estar entre 1 y 100.")]
	public int Tamano { get; set; } = 10;
}

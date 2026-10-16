using System.ComponentModel.DataAnnotations;
using SubastaYa.Domain.Enums;

namespace SubastaYa.Application.DTOs.Entrada;

public class FiltroSubastasRequest : PaginacionRequest
{
	public EstadoSubasta? Estado { get; set; }
	public int? CategoriaId { get; set; }
	public decimal? PrecioMin { get; set; }
	public decimal? PrecioMax { get; set; }
	public string? Orden { get; set; }
	[MaxLength(100)]
	public string? Busqueda { get; set; }
}

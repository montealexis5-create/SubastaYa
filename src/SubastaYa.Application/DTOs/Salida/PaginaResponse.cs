namespace SubastaYa.Application.DTOs.Salida;

public class PaginaResponse<T>
{
	public IReadOnlyList<T> Items { get; set; } = new List<T>();
	public int PaginaActual { get; set; }
	public int Tamano { get; set; }
	public int TotalElementos { get; set; }
	public int TotalPaginas { get; set; }
}

public static class PaginaResponse
{
	public static PaginaResponse<T> Crear<T>(
		IReadOnlyList<T> items, int pagina, int tamano, int totalElementos) => new()
	{
		Items = items,
		PaginaActual = pagina,
		Tamano = tamano,
		TotalElementos = totalElementos,
		TotalPaginas = (int)Math.Ceiling(totalElementos / (double)tamano)
	};
}

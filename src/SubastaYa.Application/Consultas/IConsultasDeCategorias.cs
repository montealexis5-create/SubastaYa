using SubastaYa.Application.DTOs.Salida;

namespace SubastaYa.Application.Consultas;

public interface IConsultasDeCategorias
{
	Task<IEnumerable<CategoriaResponse>> ObtenerTodasAsync();
}

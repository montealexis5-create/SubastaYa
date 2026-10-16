using SubastaYa.Application.DTOs.Entrada;
using SubastaYa.Application.DTOs.Salida;

namespace SubastaYa.Application.Consultas;

public interface IConsultasDeSubastas
{
	Task<PaginaResponse<SubastaTarjetaResponse>> ObtenerCatalogoAsync(FiltroSubastasRequest filtro);

	Task<SubastaDetalleResponse> ObtenerDetalleAsync(int id);

	Task<IReadOnlyList<MiPublicacionResponse>> ObtenerPublicacionesAsync(int vendedorId);
}

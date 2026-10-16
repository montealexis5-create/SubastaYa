using SubastaYa.Application.DTOs.Entrada;
using SubastaYa.Application.DTOs.Salida;

namespace SubastaYa.Application.Consultas;

public interface IConsultasDeBilleteras
{
	Task<BilleteraResponse> ObtenerPorUsuarioAsync(int usuarioId);

	Task<PaginaResponse<MovimientoResponse>> ObtenerMovimientosAsync(
		int usuarioId, PaginacionRequest paginacion);
}

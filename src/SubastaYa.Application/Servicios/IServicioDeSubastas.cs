using SubastaYa.Application.DTOs.Entrada;
using SubastaYa.Application.DTOs.Salida;

namespace SubastaYa.Application.Servicios;

public interface IServicioDeSubastas
{
	Task<SubastaDetalleResponse> PublicarAsync(PublicarSubastaRequest request, int vendedorId);
}

using SubastaYa.Application.DTOs.Salida;

namespace SubastaYa.Application.Servicios;

public interface IServicioDePujas
{
	Task<PujaRegistradaResponse> RegistrarPujaAsync(int subastaId, int usuarioId, decimal monto);
}

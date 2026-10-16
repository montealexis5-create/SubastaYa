using SubastaYa.Application.DTOs.Salida;

namespace SubastaYa.Application.Servicios;

public interface INotificadorDeSubastas
{
	Task NuevaPujaAsync(int subastaId, PujaRegistradaResponse puja);

	Task SubastaActivadaAsync(int subastaId, CancellationToken cancelacion);

	Task SubastaFinalizadaAsync(int subastaId, CancellationToken cancelacion);
}

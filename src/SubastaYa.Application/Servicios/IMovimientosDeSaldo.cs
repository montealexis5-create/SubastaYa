using SubastaYa.Domain.Entidades;

namespace SubastaYa.Application.Servicios;

public interface IMovimientosDeSaldo
{
	Task RetenerAsync(Billetera billetera, decimal monto, int subastaId, DateTime fecha);

	Task LiberarRetencionAsync(
		int usuarioId, decimal monto, int subastaId, DateTime fecha, CancellationToken cancelacion = default);

	Task PagarAsync(
		int compradorId, decimal monto, int subastaId, DateTime fecha, CancellationToken cancelacion = default);

	Task CobrarAsync(
		int vendedorId, decimal monto, int subastaId, DateTime fecha, CancellationToken cancelacion = default);
}

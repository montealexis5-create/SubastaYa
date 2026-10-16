using SubastaYa.Application.DTOs.Salida;

namespace SubastaYa.Application.Servicios;

public interface IServicioDeBilleteras
{
	Task<BilleteraResponse> DepositarAsync(int usuarioId, decimal monto);
}

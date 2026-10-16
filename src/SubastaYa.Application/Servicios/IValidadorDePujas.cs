using SubastaYa.Domain.Entidades;

namespace SubastaYa.Application.Servicios;

public interface IValidadorDePujas
{
	Task<(Subasta Subasta, Billetera Billetera)> ValidarAsync(int subastaId, int usuarioId, decimal monto);
}

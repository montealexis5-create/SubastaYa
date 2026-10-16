using SubastaYa.Domain.Entidades;

namespace SubastaYa.Application.Servicios;

public interface IRegistradorDePujas
{
	Task<ResultadoDeOferta> RegistrarAsync(Subasta subasta, Billetera billetera, int usuarioId, decimal monto);
}

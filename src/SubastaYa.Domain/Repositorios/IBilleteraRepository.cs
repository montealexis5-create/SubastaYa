using SubastaYa.Domain.Entidades;

namespace SubastaYa.Domain.Repositorios;

public interface IBilleteraRepository
{
	Task<Billetera?> ObtenerPorUsuarioAsync(int usuarioId, CancellationToken cancelacion = default);
}

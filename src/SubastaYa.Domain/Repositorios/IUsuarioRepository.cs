using SubastaYa.Domain.Entidades;

namespace SubastaYa.Domain.Repositorios;

public interface IUsuarioRepository
{
	Task<bool> ExisteEmailAsync(string email);

	Task<bool> ExisteSeudonimoAsync(string seudonimo);

	Task<Usuario?> ObtenerPorEmailAsync(string email);

	Task<string> ObtenerSeudonimoAsync(int id);

	void Agregar(Usuario usuario);
}

using Microsoft.EntityFrameworkCore;
using SubastaYa.Domain.Entidades;
using SubastaYa.Domain.Repositorios;
using SubastaYa.Infrastructure.Persistencia;

namespace SubastaYa.Infrastructure.Repositorios;

public class UsuarioRepository : IUsuarioRepository
{
	private readonly AppDbContext _contexto;

	public UsuarioRepository(AppDbContext contexto)
	{
		_contexto = contexto;
	}

	public Task<bool> ExisteEmailAsync(string email) => _contexto.Usuarios.AnyAsync(u => u.Email == email);

	public Task<bool> ExisteSeudonimoAsync(string seudonimo)
		=> _contexto.Usuarios.AnyAsync(u => u.Seudonimo == seudonimo);

	public Task<Usuario?> ObtenerPorEmailAsync(string email)
		=> _contexto.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Email == email);

	public Task<string> ObtenerSeudonimoAsync(int id)
	{
		return _contexto.Usuarios
			.AsNoTracking()
			.Where(u => u.Id == id)
			.Select(u => u.Seudonimo)
			.FirstAsync();
	}

	public void Agregar(Usuario usuario) => _contexto.Usuarios.Add(usuario);
}

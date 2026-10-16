using Microsoft.EntityFrameworkCore;
using SubastaYa.Domain.Repositorios;
using SubastaYa.Infrastructure.Persistencia;

namespace SubastaYa.Infrastructure.Repositorios;

public class CategoriaRepository : ICategoriaRepository
{
	private readonly AppDbContext _contexto;

	public CategoriaRepository(AppDbContext contexto)
	{
		_contexto = contexto;
	}

	public Task<bool> ExisteAsync(int id) => _contexto.Categorias.AsNoTracking().AnyAsync(c => c.Id == id);
}

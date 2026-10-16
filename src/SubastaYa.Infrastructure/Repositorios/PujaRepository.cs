using SubastaYa.Domain.Entidades;
using SubastaYa.Domain.Repositorios;
using SubastaYa.Infrastructure.Persistencia;

namespace SubastaYa.Infrastructure.Repositorios;

public class PujaRepository : IPujaRepository
{
	private readonly AppDbContext _contexto;

	public PujaRepository(AppDbContext contexto)
	{
		_contexto = contexto;
	}

	public void Agregar(Puja puja) => _contexto.Pujas.Add(puja);
}

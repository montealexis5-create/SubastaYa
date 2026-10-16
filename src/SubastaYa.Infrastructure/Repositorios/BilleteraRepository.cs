using Microsoft.EntityFrameworkCore;
using SubastaYa.Domain.Entidades;
using SubastaYa.Domain.Repositorios;
using SubastaYa.Infrastructure.Persistencia;

namespace SubastaYa.Infrastructure.Repositorios;

public class BilleteraRepository : IBilleteraRepository
{
	private readonly AppDbContext _contexto;

	public BilleteraRepository(AppDbContext contexto)
	{
		_contexto = contexto;
	}

	public Task<Billetera?> ObtenerPorUsuarioAsync(int usuarioId, CancellationToken cancelacion = default)
		=> _contexto.Billeteras.FirstOrDefaultAsync(b => b.UsuarioId == usuarioId, cancelacion);
}

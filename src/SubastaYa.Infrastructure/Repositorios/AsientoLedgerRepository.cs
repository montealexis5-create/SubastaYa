using SubastaYa.Domain.Entidades;
using SubastaYa.Domain.Repositorios;
using SubastaYa.Infrastructure.Persistencia;

namespace SubastaYa.Infrastructure.Repositorios;

public class AsientoLedgerRepository : IAsientoLedgerRepository
{
	private readonly AppDbContext _contexto;

	public AsientoLedgerRepository(AppDbContext contexto)
	{
		_contexto = contexto;
	}

	public Task AgregarAsync(AsientoLedger asiento)
	{
		_contexto.AsientosLedger.Add(asiento);
		return Task.CompletedTask;
	}
}

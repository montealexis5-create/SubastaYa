using Microsoft.EntityFrameworkCore.Storage;
using SubastaYa.Domain.Persistencia;

namespace SubastaYa.Infrastructure.Persistencia;

public class Transaccion : ITransaction
{
	private readonly IDbContextTransaction _transaccion;

	public Transaccion(IDbContextTransaction transaccion)
	{
		_transaccion = transaccion;
	}

	public Task CommitAsync(CancellationToken cancelacion = default) => _transaccion.CommitAsync(cancelacion);

	public Task RollbackAsync(CancellationToken cancelacion = default) => _transaccion.RollbackAsync(cancelacion);

	public ValueTask DisposeAsync() => _transaccion.DisposeAsync();
}

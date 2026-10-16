namespace SubastaYa.Domain.Persistencia;

public interface ITransaction : IAsyncDisposable
{
	Task CommitAsync(CancellationToken cancelacion = default);

	Task RollbackAsync(CancellationToken cancelacion = default);
}

namespace SubastaYa.Domain.Persistencia;

public interface IUnitOfWork
{
	Task<ITransaction> BeginTransactionAsync(CancellationToken cancelacion = default);

	Task SaveChangesAsync(CancellationToken cancelacion = default);

	void ClearTracking();
}

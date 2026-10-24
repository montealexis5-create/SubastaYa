using Microsoft.EntityFrameworkCore;
using SubastaYa.Domain.Excepciones;
using SubastaYa.Domain.Persistencia;

namespace SubastaYa.Infrastructure.Persistencia;

public class UnitOfWork : IUnitOfWork
{
	private readonly AppDbContext _contexto;

	public UnitOfWork(AppDbContext contexto)
	{
		_contexto = contexto;
	}

	public async Task<ITransaction> BeginTransactionAsync(CancellationToken cancelacion = default)
	{
		var transaccion = await _contexto.Database.BeginTransactionAsync(cancelacion);
		return new Transaccion(transaccion);
	}

	/// Pasa los errores de concurrencia de EF a excepciones del dominio.
	public async Task SaveChangesAsync(CancellationToken cancelacion = default)
	{
		try
		{
			await _contexto.SaveChangesAsync(cancelacion);
		}
		catch (DbUpdateConcurrencyException ex)
		{
			throw new ActualizacionConcurrenteException(ex);
		}
		catch (DbUpdateException ex) when (ex.EsViolacionDeSaldoRetenido())
		{
			throw new SaldoRetenidoNegativoException(ex);
		}
		catch (DbUpdateException ex) when (ex.EsViolacionDeUnicidad())
		{
			throw new RecursoDuplicadoException("Ya existe una cuenta con ese email o seudónimo.");
		}
	}

	public void ClearTracking() => _contexto.ChangeTracker.Clear();
}

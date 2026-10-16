using SubastaYa.Domain.Entidades;

namespace SubastaYa.Domain.Repositorios;

public interface IAsientoLedgerRepository
{
	Task AgregarAsync(AsientoLedger asiento);
	Task<decimal> SumarPorBilleteraAsync(int billeteraId);
}

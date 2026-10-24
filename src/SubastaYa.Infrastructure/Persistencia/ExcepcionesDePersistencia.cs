using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace SubastaYa.Infrastructure.Persistencia;

public static class ExcepcionesDePersistencia
{
	private const string SaldoRetenidoNoNegativo = "CK_Billetera_SaldoRetenidoNoNegativo";

	public static bool EsViolacionDeSaldoRetenido(this DbUpdateException excepcion) =>
		excepcion.InnerException is PostgresException
		{
			SqlState: PostgresErrorCodes.CheckViolation,
			ConstraintName: SaldoRetenidoNoNegativo
		};

	public static bool EsViolacionDeUnicidad(this DbUpdateException excepcion) =>
		excepcion.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}

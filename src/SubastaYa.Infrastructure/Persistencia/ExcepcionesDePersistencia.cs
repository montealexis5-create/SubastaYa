using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace SubastaYa.Infrastructure.Persistencia;

public static class ExcepcionesDePersistencia
{
	private const string SaldoRetenidoNoNegativo = "CK_Billetera_SaldoRetenidoNoNegativo";
	private const string IndiceEmail = "IX_Usuarios_Email";
	private const string IndiceSeudonimo = "IX_Usuarios_Seudonimo";

	public static bool EsViolacionDeSaldoRetenido(this DbUpdateException excepcion) =>
		excepcion.InnerException is PostgresException
		{
			SqlState: PostgresErrorCodes.CheckViolation,
			ConstraintName: SaldoRetenidoNoNegativo
		};

	// Solo los dos indices de Usuario; cualquier otra unicidad es un error inesperado.
	public static bool EsViolacionDeUsuarioDuplicado(this DbUpdateException excepcion) =>
		excepcion.InnerException is PostgresException
		{
			SqlState: PostgresErrorCodes.UniqueViolation,
			ConstraintName: IndiceEmail or IndiceSeudonimo
		};
}

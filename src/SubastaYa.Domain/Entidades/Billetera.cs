namespace SubastaYa.Domain.Entidades;

public class Billetera
{
	private const int VersionInicial = 1;

	public int Id { get; set; }
	public int UsuarioId { get; set; }
	public decimal SaldoTotal { get; set; }
	public decimal SaldoRetenido { get; set; }
	public int Version { get; set; }

	public decimal SaldoDisponible => SaldoTotal - SaldoRetenido;

	public Usuario Usuario { get; set; } = null!;

	public static Billetera Abrir(
		decimal saldoTotal = 0m, decimal saldoRetenido = 0m, int usuarioId = 0) => new()
	{
		UsuarioId = usuarioId,
		SaldoTotal = saldoTotal,
		SaldoRetenido = saldoRetenido,
		Version = VersionInicial
	};

	public void Acreditar(decimal monto)
	{
		SaldoTotal += monto;
		Version++;
	}

	/// EF no suma solo el token de concurrencia, por eso el Version++ a mano.
	public void Retener(decimal monto)
	{
		SaldoRetenido += monto;
		Version++;
	}

	public void LiberarRetencion(decimal monto)
	{
		SaldoRetenido -= monto;
		Version++;
	}

	public void PagarRetenido(decimal monto)
	{
		SaldoRetenido -= monto;
		SaldoTotal -= monto;
		Version++;
	}
}

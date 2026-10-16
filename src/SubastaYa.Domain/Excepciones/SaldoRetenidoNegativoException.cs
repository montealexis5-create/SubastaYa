namespace SubastaYa.Domain.Excepciones;

public class SaldoRetenidoNegativoException : Exception
{
	public SaldoRetenidoNegativoException(Exception causa)
		: base("La operación dejaba el saldo retenido en negativo.", causa)
	{
	}
}

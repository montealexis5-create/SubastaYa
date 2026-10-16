namespace SubastaYa.Domain.Excepciones;

public class ActualizacionConcurrenteException : Exception
{
	public ActualizacionConcurrenteException(Exception causa)
		: base("El registro cambió mientras se procesaba la operación.", causa)
	{
	}
}

namespace SubastaYa.Domain.Excepciones;

public class AccionProhibidaException : Exception
{
	public AccionProhibidaException(string mensaje) : base(mensaje) { }
}

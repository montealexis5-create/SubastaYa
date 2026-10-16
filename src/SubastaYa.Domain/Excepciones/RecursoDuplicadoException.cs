namespace SubastaYa.Domain.Excepciones;

public class RecursoDuplicadoException : Exception
{
	public RecursoDuplicadoException(string mensaje) : base(mensaje) { }
}

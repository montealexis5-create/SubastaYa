namespace SubastaYa.Domain.Excepciones;

public class RecursoNoEncontradoException : Exception
{
	public RecursoNoEncontradoException(string mensaje) : base(mensaje) { }
}

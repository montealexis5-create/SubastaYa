namespace SubastaYa.Domain.Excepciones;

public class ConflictoDeConcurrenciaException : Exception
{
	public object? Detalle { get; }

	public ConflictoDeConcurrenciaException(string mensaje, object? detalle = null)
		: base(mensaje)
	{
		Detalle = detalle;
	}
}

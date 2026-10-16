namespace SubastaYa.Domain.Repositorios;

public record EstadoDeOferta(decimal PujaActual, decimal IncrementoMinimo, DateTime FechaFin)
{
	public decimal MontoMinimo => PujaActual + IncrementoMinimo;
}

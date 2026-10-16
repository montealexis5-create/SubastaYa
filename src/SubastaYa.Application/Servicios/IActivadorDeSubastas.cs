namespace SubastaYa.Application.Servicios;

public interface IActivadorDeSubastas
{
	Task ActivarAsync(int subastaId, DateTime ahora, CancellationToken cancelacion);
}

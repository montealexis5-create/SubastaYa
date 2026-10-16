namespace SubastaYa.Application.Servicios;

public interface ICierreDeSubastas
{
	Task CerrarAsync(int subastaId, DateTime ahora, CancellationToken cancelacion);
}

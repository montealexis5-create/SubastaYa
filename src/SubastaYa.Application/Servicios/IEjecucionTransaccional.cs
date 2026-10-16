namespace SubastaYa.Application.Servicios;

public interface IEjecucionTransaccional
{
	Task<bool> EjecutarAsync(int subastaId, string operacion, Func<Task> trabajo, CancellationToken cancelacion);
}

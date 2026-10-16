namespace SubastaYa.Application.Servicios;

public interface IServicioDeAdjudicacion
{
	Task ProcesarCicloAsync(CancellationToken cancelacion);
}

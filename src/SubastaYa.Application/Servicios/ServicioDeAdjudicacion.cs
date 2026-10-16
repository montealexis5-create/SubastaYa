using SubastaYa.Domain.Repositorios;

namespace SubastaYa.Application.Servicios;

public class ServicioDeAdjudicacion : IServicioDeAdjudicacion
{
	private readonly ISubastaRepository _subastas;
	private readonly IActivadorDeSubastas _activador;
	private readonly ICierreDeSubastas _cierre;

	public ServicioDeAdjudicacion(
		ISubastaRepository subastas,
		IActivadorDeSubastas activador,
		ICierreDeSubastas cierre)
	{
		_subastas = subastas;
		_activador = activador;
		_cierre = cierre;
	}

	/// Un ciclo del worker: activa las que ya empezaron y cierra las vencidas.
	public async Task ProcesarCicloAsync(CancellationToken cancelacion)
	{
		var ahora = DateTime.UtcNow;

		var programadas = await _subastas.ObtenerIdsProgramadasParaActivarAsync(ahora, cancelacion);
		foreach (var subastaId in programadas)
		{
			await _activador.ActivarAsync(subastaId, ahora, cancelacion);
		}

		var vencidas = await _subastas.ObtenerIdsActivasVencidasAsync(ahora, cancelacion);
		foreach (var subastaId in vencidas)
		{
			await _cierre.CerrarAsync(subastaId, ahora, cancelacion);
		}
	}
}

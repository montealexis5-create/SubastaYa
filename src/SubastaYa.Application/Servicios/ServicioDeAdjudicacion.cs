using SubastaYa.Domain.Entidades;
using SubastaYa.Domain.Repositorios;

namespace SubastaYa.Application.Servicios;

public class ServicioDeAdjudicacion : IServicioDeAdjudicacion
{
	private readonly ISubastaRepository _subastas;
	private readonly IActivadorDeSubastas _activador;
	private readonly ICierreDeSubastas _cierre;
	private readonly IReloj _reloj;

	public ServicioDeAdjudicacion(
		ISubastaRepository subastas,
		IActivadorDeSubastas activador,
		ICierreDeSubastas cierre,
		IReloj reloj)
	{
		_subastas = subastas;
		_activador = activador;
		_cierre = cierre;
		_reloj = reloj;
	}

	/// Un ciclo del worker: activa las que ya empezaron y cierra las vencidas.
	public async Task ProcesarCicloAsync(CancellationToken cancelacion)
	{
		var ahora = _reloj.AhoraUtc;

		var programadas = await _subastas.ObtenerIdsAsync(Subasta.ParaActivar(ahora), cancelacion);
		foreach (var subastaId in programadas)
		{
			await _activador.ActivarAsync(subastaId, ahora, cancelacion);
		}

		var vencidas = await _subastas.ObtenerIdsAsync(Subasta.Vencida(ahora), cancelacion);
		foreach (var subastaId in vencidas)
		{
			await _cierre.CerrarAsync(subastaId, ahora, cancelacion);
		}
	}
}

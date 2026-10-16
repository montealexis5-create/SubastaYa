using Microsoft.Extensions.Options;
using SubastaYa.Api.Configuracion;
using SubastaYa.Application.Servicios;

namespace SubastaYa.Api.Workers;

public class AdjudicacionWorker : BackgroundService
{
	private const int IntervaloMinimoSegundos = 1;

	private readonly IServiceScopeFactory _scopeFactory;
	private readonly OpcionesWorker _opciones;
	private readonly ILogger<AdjudicacionWorker> _logger;

	public AdjudicacionWorker(
		IServiceScopeFactory scopeFactory,
		IOptions<OpcionesWorker> opciones,
		ILogger<AdjudicacionWorker> logger)
	{
		_scopeFactory = scopeFactory;
		_opciones = opciones.Value;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken cancelacion)
	{
		var segundos = Math.Max(IntervaloMinimoSegundos, _opciones.IntervaloSegundos);
		var intervalo = TimeSpan.FromSeconds(segundos);
		_logger.LogInformation(
			"Worker de adjudicacion iniciado, revisando cada {Segundos} segundos.", segundos);

		while (!cancelacion.IsCancellationRequested)
		{
			if (!await EjecutarCicloAsync(cancelacion) || !await EsperarAsync(intervalo, cancelacion))
			{
				break;
			}
		}

		_logger.LogInformation("Worker de adjudicacion detenido.");
	}

	private async Task<bool> EjecutarCicloAsync(CancellationToken cancelacion)
	{
		try
		{
			using var scope = _scopeFactory.CreateScope();
			var adjudicacion = scope.ServiceProvider.GetRequiredService<IServicioDeAdjudicacion>();
			await adjudicacion.ProcesarCicloAsync(cancelacion);
		}
		catch (OperationCanceledException) when (cancelacion.IsCancellationRequested)
		{
			return false;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Fallo un ciclo del worker de adjudicacion.");
		}

		return true;
	}

	private static async Task<bool> EsperarAsync(TimeSpan intervalo, CancellationToken cancelacion)
	{
		try
		{
			await Task.Delay(intervalo, cancelacion);
			return true;
		}
		catch (OperationCanceledException)
		{
			return false;
		}
	}
}

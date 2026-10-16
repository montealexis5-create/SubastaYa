using Microsoft.AspNetCore.SignalR;
using SubastaYa.Application.DTOs.Salida;
using SubastaYa.Application.Servicios;

namespace SubastaYa.Api.Hubs;

public class NotificadorSignalR : INotificadorDeSubastas
{
	private readonly IHubContext<SubastaHub> _hub;
	private readonly ILogger<NotificadorSignalR> _logger;

	public NotificadorSignalR(IHubContext<SubastaHub> hub, ILogger<NotificadorSignalR> logger)
	{
		_hub = hub;
		_logger = logger;
	}

	public Task NuevaPujaAsync(int subastaId, PujaRegistradaResponse puja)
		=> EnviarAsync(subastaId, "NuevaPuja", grupo => grupo.SendAsync("NuevaPuja", puja));

	public Task SubastaActivadaAsync(int subastaId, CancellationToken cancelacion)
		=> EnviarAsync(subastaId, "SubastaActivada",
			grupo => grupo.SendAsync("SubastaActivada", cancelacion));

	public Task SubastaFinalizadaAsync(int subastaId, CancellationToken cancelacion)
		=> EnviarAsync(subastaId, "SubastaFinalizada",
			grupo => grupo.SendAsync("SubastaFinalizada", cancelacion));

	private async Task EnviarAsync(int subastaId, string evento, Func<IClientProxy, Task> envio)
	{
		try
		{
			await envio(_hub.Clients.Group(SubastaHub.NombreDeGrupo(subastaId)));
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "No se pudo avisar {Evento} de la subasta {SubastaId}.",
				evento, subastaId);
		}
	}
}

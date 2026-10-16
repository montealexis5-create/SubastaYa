using Microsoft.AspNetCore.SignalR;

namespace SubastaYa.Api.Hubs;

public class SubastaHub : Hub
{
	public Task UnirseASubasta(int subastaId)
		=> Groups.AddToGroupAsync(Context.ConnectionId, NombreDeGrupo(subastaId));

	public Task SalirDeSubasta(int subastaId)
		=> Groups.RemoveFromGroupAsync(Context.ConnectionId, NombreDeGrupo(subastaId));

	public static string NombreDeGrupo(int subastaId) => $"subasta-{subastaId}";
}

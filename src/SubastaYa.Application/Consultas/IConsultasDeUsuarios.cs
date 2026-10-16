using SubastaYa.Application.DTOs.Salida;

namespace SubastaYa.Application.Consultas;

public interface IConsultasDeUsuarios
{
	Task<UsuarioResponse> ObtenerPorIdAsync(int id);

	Task<IReadOnlyList<MiParticipacionResponse>> ObtenerParticipacionesAsync(int usuarioId);
}

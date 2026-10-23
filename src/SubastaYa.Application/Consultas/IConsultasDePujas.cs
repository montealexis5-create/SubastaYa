using SubastaYa.Application.DTOs.Salida;

namespace SubastaYa.Application.Consultas;

public interface IConsultasDePujas
{
	Task<IReadOnlyList<PujaResponse>> ObtenerHistorialAsync(int subastaId);

	Task<int> ContarAsync(int subastaId);
}

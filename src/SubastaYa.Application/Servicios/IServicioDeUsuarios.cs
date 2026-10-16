using SubastaYa.Application.DTOs.Entrada;
using SubastaYa.Application.DTOs.Salida;

namespace SubastaYa.Application.Servicios;

public interface IServicioDeUsuarios
{
	Task<UsuarioResponse> RegistrarAsync(RegistroRequest solicitud);
}

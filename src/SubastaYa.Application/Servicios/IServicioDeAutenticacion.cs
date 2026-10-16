using SubastaYa.Application.DTOs.Entrada;
using SubastaYa.Application.DTOs.Salida;

namespace SubastaYa.Application.Servicios;

public interface IServicioDeAutenticacion
{
	Task<SesionResponse?> IniciarSesionAsync(LoginRequest solicitud);
}

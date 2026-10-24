using Microsoft.Extensions.Options;
using SubastaYa.Application.Configuracion;
using SubastaYa.Application.DTOs.Salida;

namespace SubastaYa.Application.Servicios;

public class ServicioDeConfiguracion : IServicioDeConfiguracion
{
	private readonly OpcionesPublicacion _publicacion;

	public ServicioDeConfiguracion(IOptions<OpcionesPublicacion> publicacion)
	{
		_publicacion = publicacion.Value;
	}

	public ConfiguracionPublicacionResponse ObtenerDePublicacion() => new()
	{
		DuracionMaximaDias = _publicacion.DuracionMaximaDias,
		ToleranciaInicioMinutos = _publicacion.ToleranciaInicioMinutos
	};
}

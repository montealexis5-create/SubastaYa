using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SubastaYa.Application.Configuracion;
using SubastaYa.Application.Servicios;

namespace SubastaYa.Application;

public static class InyeccionDeAplicacion
{
	public static IServiceCollection AgregarAplicacion(
		this IServiceCollection servicios, IConfiguration configuracion)
	{
		servicios.Configure<OpcionesAntiSniping>(configuracion.GetSection(OpcionesAntiSniping.Seccion));
		servicios.Configure<OpcionesPublicacion>(configuracion.GetSection(OpcionesPublicacion.Seccion));

		servicios.AddSingleton<IReloj, RelojDelSistema>();

		servicios.AddScoped<IServicioDeUsuarios, ServicioDeUsuarios>();
		servicios.AddScoped<IServicioDeAutenticacion, ServicioDeAutenticacion>();
		servicios.AddScoped<IServicioDeSubastas, ServicioDeSubastas>();
		servicios.AddScoped<IServicioDeConfiguracion, ServicioDeConfiguracion>();
		servicios.AddScoped<IServicioDeBilleteras, ServicioDeBilleteras>();
		servicios.AddScoped<IServicioDeAuditoria, ServicioDeAuditoria>();
		servicios.AddScoped<IMovimientosDeSaldo, MovimientosDeSaldo>();

		servicios.AddScoped<IServicioDePujas, ServicioDePujas>();
		servicios.AddScoped<IValidadorDePujas, ValidadorDePujas>();
		servicios.AddScoped<IRegistradorDePujas, RegistradorDePujas>();

		servicios.AddScoped<IServicioDeAdjudicacion, ServicioDeAdjudicacion>();
		servicios.AddScoped<IActivadorDeSubastas, ActivadorDeSubastas>();
		servicios.AddScoped<ICierreDeSubastas, CierreDeSubastas>();
		servicios.AddScoped<IEjecucionTransaccional, EjecucionTransaccional>();

		return servicios;
	}
}

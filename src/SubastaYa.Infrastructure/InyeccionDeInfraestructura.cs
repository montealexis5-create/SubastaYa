using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SubastaYa.Application.Consultas;
using SubastaYa.Application.Servicios;
using SubastaYa.Domain.Persistencia;
using SubastaYa.Domain.Repositorios;
using SubastaYa.Infrastructure.Consultas;
using SubastaYa.Infrastructure.Persistencia;
using SubastaYa.Infrastructure.Repositorios;
using SubastaYa.Infrastructure.Seguridad;

namespace SubastaYa.Infrastructure;

public static class InyeccionDeInfraestructura
{
	public static IServiceCollection AgregarInfraestructura(
		this IServiceCollection servicios, string? cadenaDeConexion)
	{
		servicios.AddDbContext<AppDbContext>(opciones => opciones.UseNpgsql(cadenaDeConexion));
		servicios.AddScoped<IUnitOfWork, UnitOfWork>();

		servicios.AddScoped<IServicioDePasswords, ServicioDePasswords>();

		servicios.AddScoped<IUsuarioRepository, UsuarioRepository>();
		servicios.AddScoped<IBilleteraRepository, BilleteraRepository>();
		servicios.AddScoped<ISubastaRepository, SubastaRepository>();
		servicios.AddScoped<IPujaRepository, PujaRepository>();
		servicios.AddScoped<ICategoriaRepository, CategoriaRepository>();
		servicios.AddScoped<IAsientoLedgerRepository, AsientoLedgerRepository>();
		servicios.AddScoped<IRegistroAuditoriaRepository, RegistroAuditoriaRepository>();

		servicios.AddScoped<IConsultasDeSubastas, ConsultasDeSubastas>();
		servicios.AddScoped<IConsultasDePujas, ConsultasDePujas>();
		servicios.AddScoped<IConsultasDeCategorias, ConsultasDeCategorias>();
		servicios.AddScoped<IConsultasDeUsuarios, ConsultasDeUsuarios>();
		servicios.AddScoped<IConsultasDeBilleteras, ConsultasDeBilleteras>();

		return servicios;
	}
}

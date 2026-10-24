using SubastaYa.Api.Configuracion;
using SubastaYa.Api.Extensiones;
using SubastaYa.Api.Hubs;
using SubastaYa.Api.Middleware;
using SubastaYa.Api.Servicios;
using SubastaYa.Api.Workers;
using SubastaYa.Application;
using SubastaYa.Application.Servicios;
using SubastaYa.Infrastructure;
using SubastaYa.Infrastructure.Seed;

var builder = WebApplication.CreateBuilder(args);
var servicios = builder.Services;
var configuracion = builder.Configuration;

servicios.AgregarInfraestructura(configuracion.GetConnectionString("Default"));
servicios.AgregarAplicacion(configuracion);
servicios.AgregarAutenticacionJwt(configuracion);
servicios.AgregarSwaggerConJwt();
servicios.AgregarCorsDelFrontend();

servicios.AddScoped<ITokenService, TokenService>();
servicios.AddSingleton<INotificadorDeSubastas, NotificadorSignalR>();

servicios.Configure<OpcionesWorker>(configuracion.GetSection(OpcionesWorker.Seccion));
servicios.AddHostedService<AdjudicacionWorker>();

servicios.AddSignalR();
servicios.AddControllers();

var app = builder.Build();

app.UseMiddleware<ManejadorDeExcepcionesMiddleware>();

// Solo en desarrollo: Swagger y los datos de prueba.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
	app.Services.AplicarDatosSemilla();
}

app.UseCors(PoliticaCors.Frontend);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<SubastaHub>("/hubs/subastas");
app.Run();

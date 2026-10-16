namespace SubastaYa.Api.Extensiones;

public static class PoliticaCors
{
	public const string Frontend = "Frontend";

	private static readonly string[] OrigenesDelFrontend =
	{
		"http://127.0.0.1:5500",
		"http://localhost:5500"
	};

	public static IServiceCollection AgregarCorsDelFrontend(this IServiceCollection servicios)
	{
		servicios.AddCors(opciones => opciones.AddPolicy(Frontend, politica =>
			politica.WithOrigins(OrigenesDelFrontend)
				.AllowAnyHeader()
				.AllowAnyMethod()
				.AllowCredentials()));

		return servicios;
	}
}

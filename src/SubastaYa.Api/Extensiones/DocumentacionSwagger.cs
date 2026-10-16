using Microsoft.OpenApi;

namespace SubastaYa.Api.Extensiones;

public static class DocumentacionSwagger
{
	private const string EsquemaBearer = "Bearer";

	public static IServiceCollection AgregarSwaggerConJwt(this IServiceCollection servicios)
	{
		servicios.AddEndpointsApiExplorer();

		servicios.AddSwaggerGen(opciones =>
		{
			opciones.AddSecurityDefinition(EsquemaBearer, new OpenApiSecurityScheme
			{
				Name = "Authorization",
				Type = SecuritySchemeType.Http,
				Scheme = "bearer",
				BearerFormat = "JWT",
				In = ParameterLocation.Header,
				Description = "Pegar solamente el token, sin escribir 'Bearer' adelante."
			});

			opciones.AddSecurityRequirement(documento => new OpenApiSecurityRequirement
			{
				{ new OpenApiSecuritySchemeReference(EsquemaBearer, documento), new List<string>() }
			});
		});

		return servicios;
	}
}

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace SubastaYa.Api.Extensiones;

public static class AutenticacionJwt
{
	public static IServiceCollection AgregarAutenticacionJwt(
		this IServiceCollection servicios, IConfiguration configuracion)
	{
		servicios.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(opciones =>
			{
				opciones.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = configuracion["Jwt:Issuer"],
					ValidAudience = configuracion["Jwt:Audience"],
					IssuerSigningKey = new SymmetricSecurityKey(
						Encoding.UTF8.GetBytes(configuracion["Jwt:Clave"]!)),
					ClockSkew = TimeSpan.Zero
				};

				opciones.MapInboundClaims = false;
			});

		servicios.AddAuthorization();

		return servicios;
	}
}

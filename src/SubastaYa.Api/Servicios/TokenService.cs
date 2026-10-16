using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SubastaYa.Application.Servicios;
using SubastaYa.Domain.Entidades;

namespace SubastaYa.Api.Servicios;

public class TokenService : ITokenService
{
	private static readonly JwtSecurityTokenHandler Handler = new();

	private readonly string _clave;
	private readonly string _emisor;
	private readonly string _destinatario;
	private readonly int _minutosDeVigencia;

	public TokenService(IConfiguration configuracion)
	{
		_clave = Requerido(configuracion, "Jwt:Clave");
		_emisor = Requerido(configuracion, "Jwt:Issuer");
		_destinatario = Requerido(configuracion, "Jwt:Audience");
		_minutosDeVigencia = int.Parse(Requerido(configuracion, "Jwt:ExpiracionMinutos"));
	}

	public (string Token, DateTime ExpiraEn) GenerarToken(Usuario usuario)
	{
		var ahora = DateTime.UtcNow;
		var expiraEn = ahora.AddMinutes(_minutosDeVigencia);

		var claims = new[]
		{
			new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
			new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
			new Claim("nombre", usuario.Nombre)
		};

		var credenciales = new SigningCredentials(
			new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_clave)),
			SecurityAlgorithms.HmacSha256);

		var token = new JwtSecurityToken(
			issuer: _emisor,
			audience: _destinatario,
			claims: claims,
			notBefore: ahora,
			expires: expiraEn,
			signingCredentials: credenciales);

		return (Handler.WriteToken(token), expiraEn);
	}

	private static string Requerido(IConfiguration configuracion, string clave) =>
		configuracion[clave]
		?? throw new InvalidOperationException($"Falta la configuracion '{clave}'.");
}

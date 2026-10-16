using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SubastaYa.Api.Servicios;

public static class UsuarioActual
{
	public static int ObtenerId(ClaimsPrincipal usuario)
	{
		var sub = usuario.FindFirstValue(JwtRegisteredClaimNames.Sub);

		return int.TryParse(sub, out var id)
			? id
			: throw new InvalidOperationException("El token no trae un claim 'sub' numerico.");
	}
}

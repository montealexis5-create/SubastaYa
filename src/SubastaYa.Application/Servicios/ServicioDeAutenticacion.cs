using SubastaYa.Application.DTOs.Entrada;
using SubastaYa.Application.DTOs.Salida;
using SubastaYa.Domain.Repositorios;

namespace SubastaYa.Application.Servicios;

public class ServicioDeAutenticacion : IServicioDeAutenticacion
{
	private readonly IUsuarioRepository _usuarios;
	private readonly IServicioDePasswords _passwords;
	private readonly ITokenService _tokens;

	public ServicioDeAutenticacion(
		IUsuarioRepository usuarios,
		IServicioDePasswords passwords,
		ITokenService tokens)
	{
		_usuarios = usuarios;
		_passwords = passwords;
		_tokens = tokens;
	}

	public async Task<SesionResponse?> IniciarSesionAsync(LoginRequest solicitud)
	{
		var email = solicitud.Email.Trim().ToLowerInvariant();
		var usuario = await _usuarios.ObtenerPorEmailAsync(email);

		var passwordValida = _passwords.Verificar(
			solicitud.Password,
			usuario?.PasswordHash ?? _passwords.HashDeDescarte);

		if (usuario is null || !passwordValida)
		{
			return null;
		}

		var (token, expiraEn) = _tokens.GenerarToken(usuario);

		return new SesionResponse
		{
			Token = token,
			ExpiraEn = expiraEn,
			Usuario = UsuarioResponse.Desde(usuario)
		};
	}
}

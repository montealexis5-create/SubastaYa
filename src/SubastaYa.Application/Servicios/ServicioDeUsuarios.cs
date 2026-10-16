using SubastaYa.Application.DTOs.Entrada;
using SubastaYa.Application.DTOs.Salida;
using SubastaYa.Domain.Entidades;
using SubastaYa.Domain.Excepciones;
using SubastaYa.Domain.Persistencia;
using SubastaYa.Domain.Repositorios;

namespace SubastaYa.Application.Servicios;

public class ServicioDeUsuarios : IServicioDeUsuarios
{
	private readonly IUsuarioRepository _usuarios;
	private readonly IServicioDePasswords _passwords;
	private readonly IUnitOfWork _unidad;

	public ServicioDeUsuarios(
		IUsuarioRepository usuarios,
		IServicioDePasswords passwords,
		IUnitOfWork unidad)
	{
		_usuarios = usuarios;
		_passwords = passwords;
		_unidad = unidad;
	}

	public async Task<UsuarioResponse> RegistrarAsync(RegistroRequest solicitud)
	{
		var email = solicitud.Email.Trim().ToLowerInvariant();
		var seudonimo = solicitud.Seudonimo.Trim();

		await ValidarDisponibilidadAsync(email, seudonimo);

		await using var transaccion = await _unidad.BeginTransactionAsync();

		var usuario = new Usuario
		{
			Email = email,
			PasswordHash = _passwords.Hashear(solicitud.Password),
			Nombre = solicitud.Nombre.Trim(),
			Seudonimo = seudonimo,
			FechaRegistro = DateTime.UtcNow,
			Billetera = Billetera.Abrir()
		};

		_usuarios.Agregar(usuario);
		await _unidad.SaveChangesAsync();
		await transaccion.CommitAsync();

		return UsuarioResponse.Desde(usuario);
	}

	private async Task ValidarDisponibilidadAsync(string email, string seudonimo)
	{
		if (await _usuarios.ExisteEmailAsync(email))
		{
			throw new RecursoDuplicadoException("Ya existe una cuenta registrada con ese email.");
		}

		if (await _usuarios.ExisteSeudonimoAsync(seudonimo))
		{
			throw new RecursoDuplicadoException("Ese seudónimo ya está en uso.");
		}
	}
}

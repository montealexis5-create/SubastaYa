using SubastaYa.Domain.Entidades;

namespace SubastaYa.Application.DTOs.Salida;

public class UsuarioResponse
{
	public int Id { get; set; }
	public string Email { get; set; } = null!;
	public string Nombre { get; set; } = null!;
	public string Seudonimo { get; set; } = null!;
	public DateTime FechaRegistro { get; set; }

	public static UsuarioResponse Desde(Usuario usuario) => new()
	{
		Id = usuario.Id,
		Email = usuario.Email,
		Nombre = usuario.Nombre,
		Seudonimo = usuario.Seudonimo,
		FechaRegistro = usuario.FechaRegistro
	};
}

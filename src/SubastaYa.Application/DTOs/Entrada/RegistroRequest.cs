using System.ComponentModel.DataAnnotations;

namespace SubastaYa.Application.DTOs.Entrada;

public class RegistroRequest
{
	[Required(ErrorMessage = "El email es obligatorio.")]
	[EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
	[MaxLength(150)]
	public string Email { get; set; } = null!;

	[Required(ErrorMessage = "La contraseña es obligatoria.")]
	[MinLength(8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
	public string Password { get; set; } = null!;

	[Required(ErrorMessage = "El nombre es obligatorio.")]
	[MaxLength(100)]
	public string Nombre { get; set; } = null!;

	[Required(ErrorMessage = "El seudónimo es obligatorio.")]
	[MaxLength(50)]
	public string Seudonimo { get; set; } = null!;
}

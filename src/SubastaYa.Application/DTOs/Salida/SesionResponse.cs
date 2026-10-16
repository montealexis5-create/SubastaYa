namespace SubastaYa.Application.DTOs.Salida;

public class SesionResponse
{
	public string Token { get; set; } = null!;

	public DateTime ExpiraEn { get; set; }

	public UsuarioResponse Usuario { get; set; } = null!;
}

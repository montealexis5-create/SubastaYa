namespace SubastaYa.Application.DTOs.Salida;

public class ErrorResponse
{
	public string Mensaje { get; set; } = null!;

	public ErrorResponse() { }

	public ErrorResponse(string mensaje) => Mensaje = mensaje;
}

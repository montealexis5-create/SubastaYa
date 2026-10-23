namespace SubastaYa.Application.Configuracion;

public class OpcionesPublicacion
{
	public const string Seccion = "Publicacion";

	public int DuracionMaximaDias { get; set; } = 365;

	public int ToleranciaInicioMinutos { get; set; } = 5;
}

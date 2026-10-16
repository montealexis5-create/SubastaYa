namespace SubastaYa.Application.Configuracion;

public class OpcionesAntiSniping
{
	public const string Seccion = "AntiSniping";

	public int UmbralSegundos { get; set; } = 60;

	public int ExtensionMinutos { get; set; } = 2;
}

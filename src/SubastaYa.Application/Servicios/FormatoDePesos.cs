using System.Globalization;

namespace SubastaYa.Application.Servicios;

public static class FormatoDePesos
{
	private static readonly CultureInfo CulturaArgentina = CultureInfo.GetCultureInfo("es-AR");

	public static string Formatear(decimal monto) => monto.ToString("C", CulturaArgentina);
}

namespace SubastaYa.Application.DTOs.Entrada;

internal static class ValidacionDeMontos
{
	private const int DecimalesPermitidos = 2;

	public static bool TieneDemasiadosDecimales(decimal monto)
		=> decimal.Round(monto, DecimalesPermitidos) != monto;
}

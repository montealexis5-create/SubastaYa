using SubastaYa.Domain.Entidades;
using Xunit;

namespace SubastaYa.Domain.Tests;

public class BilleteraTests
{
	[Fact]
	public void Abrir_empieza_en_version_uno_y_sin_retenido()
	{
		var billetera = Billetera.Abrir(500m);

		Assert.Equal(500m, billetera.SaldoTotal);
		Assert.Equal(0m, billetera.SaldoRetenido);
		Assert.Equal(1, billetera.Version);
	}

	[Fact]
	public void Retener_baja_el_disponible_y_no_toca_el_total()
	{
		var billetera = Billetera.Abrir(1000m);

		billetera.Retener(300m);

		Assert.Equal(1000m, billetera.SaldoTotal);
		Assert.Equal(300m, billetera.SaldoRetenido);
		Assert.Equal(700m, billetera.SaldoDisponible);
	}

	[Fact]
	public void LiberarRetencion_devuelve_el_disponible()
	{
		var billetera = Billetera.Abrir(1000m, 300m);

		billetera.LiberarRetencion(300m);

		Assert.Equal(0m, billetera.SaldoRetenido);
		Assert.Equal(1000m, billetera.SaldoDisponible);
	}

	[Fact]
	public void PagarRetenido_baja_total_y_retenido()
	{
		var billetera = Billetera.Abrir(1000m, 300m);

		billetera.PagarRetenido(300m);

		Assert.Equal(700m, billetera.SaldoTotal);
		Assert.Equal(0m, billetera.SaldoRetenido);
	}

	[Fact]
	public void Cada_movimiento_suma_una_version()
	{
		var billetera = Billetera.Abrir(1000m);

		billetera.Acreditar(100m);
		billetera.Retener(50m);
		billetera.LiberarRetencion(50m);
		billetera.PagarRetenido(0m);

		Assert.Equal(5, billetera.Version);
	}
}

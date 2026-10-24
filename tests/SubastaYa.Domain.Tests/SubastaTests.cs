using SubastaYa.Domain.Entidades;
using SubastaYa.Domain.Enums;
using Xunit;

namespace SubastaYa.Domain.Tests;

public class SubastaTests
{
	private const int Umbral = 60;
	private const int Extension = 2;
	private static readonly DateTime Cierre = new(2030, 1, 1, 12, 0, 0);

	private static Subasta NuevaSubasta() => new()
	{
		PujaActual = 1000m,
		IncrementoMinimo = 100m,
		FechaFin = Cierre,
		Estado = EstadoSubasta.Activa,
		Version = 1
	};

	[Fact]
	public void MontoMinimo_es_puja_actual_mas_incremento()
	{
		var subasta = NuevaSubasta();

		Assert.Equal(1100m, subasta.MontoMinimo);
	}

	[Fact]
	public void RecibirOferta_guarda_monto_y_lider()
	{
		var subasta = NuevaSubasta();

		subasta.RecibirOferta(7, 1100m, Cierre.AddMinutes(-30), Umbral, Extension);

		Assert.Equal(1100m, subasta.PujaActual);
		Assert.Equal(7, subasta.LiderId);
		Assert.Equal(1200m, subasta.MontoMinimo);
	}

	[Fact]
	public void RecibirOferta_lejos_del_cierre_no_extiende()
	{
		var subasta = NuevaSubasta();

		var extendio = subasta.RecibirOferta(7, 1100m, Cierre.AddMinutes(-30), Umbral, Extension);

		Assert.False(extendio);
		Assert.Equal(Cierre, subasta.FechaFin);
	}

	[Fact]
	public void RecibirOferta_en_los_ultimos_segundos_extiende()
	{
		var subasta = NuevaSubasta();

		var extendio = subasta.RecibirOferta(7, 1100m, Cierre.AddSeconds(-30), Umbral, Extension);

		Assert.True(extendio);
		Assert.Equal(Cierre.AddMinutes(Extension), subasta.FechaFin);
	}

	[Fact]
	public void RecibirOferta_justo_en_el_umbral_extiende()
	{
		var subasta = NuevaSubasta();

		var extendio = subasta.RecibirOferta(7, 1100m, Cierre.AddSeconds(-Umbral), Umbral, Extension);

		Assert.True(extendio);
	}

	[Fact]
	public void RecibirOferta_un_segundo_antes_del_umbral_no_extiende()
	{
		var subasta = NuevaSubasta();

		var extendio = subasta.RecibirOferta(7, 1100m, Cierre.AddSeconds(-(Umbral + 1)), Umbral, Extension);

		Assert.False(extendio);
	}

	[Fact]
	public void RecibirOferta_suma_una_version()
	{
		var subasta = NuevaSubasta();

		subasta.RecibirOferta(7, 1100m, Cierre.AddMinutes(-30), Umbral, Extension);

		Assert.Equal(2, subasta.Version);
	}

	[Fact]
	public void Activar_cambia_el_estado_y_suma_version()
	{
		var subasta = NuevaSubasta();
		subasta.Estado = EstadoSubasta.Programada;

		subasta.Activar();

		Assert.Equal(EstadoSubasta.Activa, subasta.Estado);
		Assert.Equal(2, subasta.Version);
	}

	[Fact]
	public void Finalizar_cambia_el_estado_y_suma_version()
	{
		var subasta = NuevaSubasta();

		subasta.Finalizar();

		Assert.Equal(EstadoSubasta.Finalizada, subasta.Estado);
		Assert.Equal(2, subasta.Version);
	}

	[Fact]
	public void DeclararDesierta_cambia_el_estado_y_suma_version()
	{
		var subasta = NuevaSubasta();

		subasta.DeclararDesierta();

		Assert.Equal(EstadoSubasta.Desierta, subasta.Estado);
		Assert.Equal(2, subasta.Version);
	}
}

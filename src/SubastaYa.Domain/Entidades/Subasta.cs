using System.Linq.Expressions;
using SubastaYa.Domain.Enums;

namespace SubastaYa.Domain.Entidades;

public class Subasta
{
	public int Id { get; set; }
	public int VendedorId { get; set; }
	public int CategoriaId { get; set; }
	public string Titulo { get; set; } = null!;
	public string Descripcion { get; set; } = null!;
	public string UrlImagen { get; set; } = null!;
	public decimal PrecioBase { get; set; }
	public decimal IncrementoMinimo { get; set; }
	public decimal PujaActual { get; set; }
	public int? LiderId { get; set; }
	public DateTime FechaInicio { get; set; }
	public DateTime FechaFin { get; set; }
	public EstadoSubasta Estado { get; set; }
	public int Version { get; set; }
	public DateTime FechaCreacion { get; set; }

	public Usuario Vendedor { get; set; } = null!;
	public Categoria Categoria { get; set; } = null!;
	public Usuario? Lider { get; set; }
	public ICollection<Puja> Pujas { get; set; } = new List<Puja>();

	public decimal MontoMinimo => CalcularMontoMinimo(PujaActual, IncrementoMinimo);

	/// Unica formula del minimo para ofertar. La usan la entidad y las respuestas.
	public static decimal CalcularMontoMinimo(decimal pujaActual, decimal incrementoMinimo)
		=> pujaActual + incrementoMinimo;

	/// Programada cuyo inicio ya llego.
	public static Expression<Func<Subasta, bool>> ParaActivar(DateTime ahora)
		=> s => s.Estado == EstadoSubasta.Programada && s.FechaInicio <= ahora;

	/// Activa cuyo fin ya paso.
	public static Expression<Func<Subasta, bool>> Vencida(DateTime ahora)
		=> s => s.Estado == EstadoSubasta.Activa && s.FechaFin <= ahora;

	/// Anti-sniping: si la oferta llega sobre el cierre, corre la fecha de fin.
	/// Devuelve true si extendio el tiempo.
	public bool RecibirOferta(
		int compradorId, decimal monto, DateTime ahora, int umbralSegundos, int extensionMinutos)
	{
		PujaActual = monto;
		LiderId = compradorId;

		var tiempoExtendido = (FechaFin - ahora).TotalSeconds <= umbralSegundos;
		if (tiempoExtendido)
		{
			FechaFin = FechaFin.AddMinutes(extensionMinutos);
		}

		Version++;
		return tiempoExtendido;
	}

	public void Activar() => CambiarEstado(EstadoSubasta.Activa);

	public void Finalizar() => CambiarEstado(EstadoSubasta.Finalizada);

	public void DeclararDesierta() => CambiarEstado(EstadoSubasta.Desierta);

	private void CambiarEstado(EstadoSubasta nuevoEstado)
	{
		Estado = nuevoEstado;
		Version++;
	}
}

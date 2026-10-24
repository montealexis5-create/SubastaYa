using SubastaYa.Domain.Enums;

namespace SubastaYa.Application.Servicios;

/// Lo que le toca a cada parte segun como quedo la subasta.
public static class ResultadosDeSubasta
{
	public static string ParaComprador(EstadoSubasta estado, bool esLider)
	{
		if (estado == EstadoSubasta.Finalizada)
		{
			return esLider ? "Ganada" : "No ganada";
		}

		return esLider ? "Liderando" : "Superado";
	}

	public static string ParaVendedor(EstadoSubasta estado) => estado switch
	{
		EstadoSubasta.Finalizada => "Adjudicada",
		EstadoSubasta.Desierta => "Desierta",
		EstadoSubasta.Programada => "Programada",
		_ => "En curso"
	};

	public static decimal Recaudado(EstadoSubasta estado, decimal pujaActual)
		=> (estado == EstadoSubasta.Finalizada) ? pujaActual : 0m;

	public static string? Ganador(EstadoSubasta estado, string? seudonimoLider)
		=> (estado == EstadoSubasta.Finalizada) ? seudonimoLider : null;
}

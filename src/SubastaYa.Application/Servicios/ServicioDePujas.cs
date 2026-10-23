using SubastaYa.Application.Consultas;
using SubastaYa.Application.DTOs.Salida;
using SubastaYa.Domain.Entidades;
using SubastaYa.Domain.Repositorios;

namespace SubastaYa.Application.Servicios;

public class ServicioDePujas : IServicioDePujas
{
	private readonly IValidadorDePujas _validador;
	private readonly IRegistradorDePujas _registrador;
	private readonly IUsuarioRepository _usuarios;
	private readonly INotificadorDeSubastas _notificador;
	private readonly IConsultasDePujas _consultas;

	public ServicioDePujas(
		IValidadorDePujas validador,
		IRegistradorDePujas registrador,
		IUsuarioRepository usuarios,
		INotificadorDeSubastas notificador,
		IConsultasDePujas consultas)
	{
		_validador = validador;
		_registrador = registrador;
		_usuarios = usuarios;
		_notificador = notificador;
		_consultas = consultas;
	}

	/// Valida la oferta, mueve la plata y avisa a la sala.
	/// Si otra oferta le gana de mano, termina en un 409.
	public async Task<PujaRegistradaResponse> RegistrarPujaAsync(int subastaId, int usuarioId, decimal monto)
	{
		var (subasta, billetera) = await _validador.ValidarAsync(subastaId, usuarioId, monto);
		var resultado = await _registrador.RegistrarAsync(subasta, billetera, usuarioId, monto);

		var respuesta = await ArmarRespuestaAsync(resultado, subasta, usuarioId);
		await _notificador.NuevaPujaAsync(subasta.Id, respuesta);

		return respuesta;
	}

	private async Task<PujaRegistradaResponse> ArmarRespuestaAsync(
		ResultadoDeOferta resultado, Subasta subasta, int usuarioId)
	{
		var seudonimo = await _usuarios.ObtenerSeudonimoAsync(usuarioId);

		return new PujaRegistradaResponse
		{
			Id = resultado.Puja.Id,
			Monto = resultado.Puja.Monto,
			FechaPuja = resultado.Puja.FechaPuja,
			Seudonimo = seudonimo,
			CantidadOfertas = await _consultas.ContarAsync(subasta.Id),
			FechaFin = subasta.FechaFin,
			MontoMinimo = subasta.MontoMinimo,
			TiempoExtendido = resultado.TiempoExtendido
		};
	}
}

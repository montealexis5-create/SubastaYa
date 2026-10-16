using System.Text.Encodings.Web;
using System.Text.Json;
using SubastaYa.Domain.Entidades;
using SubastaYa.Domain.Repositorios;

namespace SubastaYa.Application.Servicios;

public class ServicioDeAuditoria : IServicioDeAuditoria
{
	private const string DetalleVacio = "{}";

	private static readonly JsonSerializerOptions OpcionesDetalle =
		new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

	private readonly IRegistroAuditoriaRepository _repositorio;

	public ServicioDeAuditoria(IRegistroAuditoriaRepository repositorio)
	{
		_repositorio = repositorio;
	}

	public async Task RegistrarAsync(
		string entidad, int entidadId, string accion, int? usuarioId, object? detalle = null)
	{
		await _repositorio.AgregarAsync(new RegistroAuditoria
		{
			Entidad = entidad,
			EntidadId = entidadId,
			Accion = accion,
			UsuarioId = usuarioId,
			DetalleJson = (detalle is null) ? DetalleVacio : JsonSerializer.Serialize(detalle, OpcionesDetalle),
			Fecha = DateTime.UtcNow
		});
	}
}

using System.Text.Encodings.Web;
using System.Text.Json;
using SubastaYa.Application.DTOs.Salida;
using SubastaYa.Domain.Excepciones;

namespace SubastaYa.Api.Middleware;

public class ManejadorDeExcepcionesMiddleware
{
	private const string MensajeInesperado = "Ocurrió un error inesperado procesando la petición.";
	private const string TipoJson = "application/json";

	private static readonly JsonSerializerOptions OpcionesJson =
		new(JsonSerializerDefaults.Web) { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

	private readonly RequestDelegate _siguiente;
	private readonly ILogger<ManejadorDeExcepcionesMiddleware> _logger;

	public ManejadorDeExcepcionesMiddleware(
		RequestDelegate siguiente,
		ILogger<ManejadorDeExcepcionesMiddleware> logger)
	{
		_siguiente = siguiente;
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext contexto)
	{
		try
		{
			await _siguiente(contexto);
		}
		catch (Exception ex)
		{
			if (contexto.Response.HasStarted)
			{
				_logger.LogError(ex, "Excepcion despues de iniciada la respuesta.");
				throw;
			}

			var (codigo, cuerpo) = Traducir(ex);

			if (codigo == StatusCodes.Status500InternalServerError)
			{
				_logger.LogError(ex, "Excepcion no controlada.");
			}

			contexto.Response.Clear();
			contexto.Response.StatusCode = codigo;
			contexto.Response.ContentType = TipoJson;

			await contexto.Response.WriteAsync(
				JsonSerializer.Serialize(cuerpo, OpcionesJson));
		}
	}

	private static (int Codigo, object Cuerpo) Traducir(Exception ex) => ex switch
	{
		RecursoNoEncontradoException =>
			(StatusCodes.Status404NotFound, Mensaje(ex)),

		ReglaDeNegocioException =>
			(StatusCodes.Status422UnprocessableEntity, Mensaje(ex)),

		AccionProhibidaException =>
			(StatusCodes.Status403Forbidden, Mensaje(ex)),

		ConflictoDeConcurrenciaException c =>
			(StatusCodes.Status409Conflict, c.Detalle ?? Mensaje(ex)),

		RecursoDuplicadoException =>
			(StatusCodes.Status409Conflict, Mensaje(ex)),

		_ => (StatusCodes.Status500InternalServerError, (object)new ErrorResponse(MensajeInesperado))
	};

	private static object Mensaje(Exception ex) => new ErrorResponse(ex.Message);
}

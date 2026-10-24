using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.Consultas;
using SubastaYa.Application.DTOs.Salida;
using SubastaYa.Application.Servicios;
using SubastaYa.Domain.Enums;
using SubastaYa.Domain.Excepciones;
using SubastaYa.Infrastructure.Persistencia;

namespace SubastaYa.Infrastructure.Consultas;

public class ConsultasDeUsuarios : IConsultasDeUsuarios
{
	private readonly AppDbContext _contexto;

	public ConsultasDeUsuarios(AppDbContext contexto)
	{
		_contexto = contexto;
	}

	public async Task<UsuarioResponse> ObtenerPorIdAsync(int id)
	{
		var usuario = await _contexto.Usuarios
			.AsNoTracking()
			.FirstOrDefaultAsync(u => u.Id == id);

		if (usuario is null)
		{
			throw new RecursoNoEncontradoException("El usuario no existe.");
		}

		return UsuarioResponse.Desde(usuario);
	}

	public async Task<IReadOnlyList<MiParticipacionResponse>> ObtenerParticipacionesAsync(int usuarioId)
	{
		var subastas = await _contexto.Subastas
			.AsNoTracking()
			.Where(s => s.Pujas.Any(p => p.CompradorId == usuarioId))
			.Select(s => new
			{
				s.Id,
				s.Titulo,
				s.UrlImagen,
				s.Estado,
				s.PujaActual,
				s.FechaFin,
				s.LiderId,
				MiOfertaMaxima = s.Pujas
					.Where(p => p.CompradorId == usuarioId)
					.Max(p => p.Monto)
			})
			.ToListAsync();

		var abiertas = subastas
			.Where(s => s.Estado == EstadoSubasta.Activa)
			.OrderBy(s => s.FechaFin);
		var cerradas = subastas
			.Where(s => s.Estado != EstadoSubasta.Activa)
			.OrderByDescending(s => s.FechaFin);

		return abiertas.Concat(cerradas)
			.Select(s => new MiParticipacionResponse
			{
				SubastaId = s.Id,
				Titulo = s.Titulo,
				UrlImagen = s.UrlImagen,
				Estado = s.Estado.ToString(),
				MiOfertaMaxima = s.MiOfertaMaxima,
				PujaActual = s.PujaActual,
				FechaFin = s.FechaFin,
				Resultado = ResultadosDeSubasta.ParaComprador(s.Estado, s.LiderId == usuarioId)
			})
			.ToList();
	}
}

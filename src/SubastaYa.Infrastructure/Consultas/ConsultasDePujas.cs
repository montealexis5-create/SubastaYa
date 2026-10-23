using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.Consultas;
using SubastaYa.Application.DTOs.Salida;
using SubastaYa.Domain.Excepciones;
using SubastaYa.Infrastructure.Persistencia;

namespace SubastaYa.Infrastructure.Consultas;

public class ConsultasDePujas : IConsultasDePujas
{
	private readonly AppDbContext _contexto;

	public ConsultasDePujas(AppDbContext contexto)
	{
		_contexto = contexto;
	}

	public async Task<IReadOnlyList<PujaResponse>> ObtenerHistorialAsync(int subastaId)
	{
		if (!await _contexto.Subastas.AnyAsync(s => s.Id == subastaId))
		{
			throw new RecursoNoEncontradoException("La subasta no existe.");
		}

		return await _contexto.Pujas
			.AsNoTracking()
			.Where(p => p.SubastaId == subastaId)
			.OrderByDescending(p => p.FechaPuja)
			.ThenByDescending(p => p.Id)
			.Select(p => new PujaResponse
			{
				Id = p.Id,
				Monto = p.Monto,
				FechaPuja = p.FechaPuja,
				Seudonimo = p.Comprador.Seudonimo
			})
			.ToListAsync();
	}

	public Task<int> ContarAsync(int subastaId)
		=> _contexto.Pujas.CountAsync(p => p.SubastaId == subastaId);
}

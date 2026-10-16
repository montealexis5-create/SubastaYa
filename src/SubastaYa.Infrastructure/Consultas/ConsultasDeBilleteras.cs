using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.Consultas;
using SubastaYa.Application.DTOs.Entrada;
using SubastaYa.Application.DTOs.Salida;
using SubastaYa.Domain.Excepciones;
using SubastaYa.Infrastructure.Persistencia;

namespace SubastaYa.Infrastructure.Consultas;

public class ConsultasDeBilleteras : IConsultasDeBilleteras
{
	private const string SinBilletera = "El usuario no tiene una billetera asociada.";

	private readonly AppDbContext _contexto;

	public ConsultasDeBilleteras(AppDbContext contexto)
	{
		_contexto = contexto;
	}

	public async Task<BilleteraResponse> ObtenerPorUsuarioAsync(int usuarioId)
	{
		var billetera = await _contexto.Billeteras
			.AsNoTracking()
			.FirstOrDefaultAsync(b => b.UsuarioId == usuarioId)
			?? throw new RecursoNoEncontradoException(SinBilletera);

		return BilleteraResponse.Desde(billetera);
	}

	public async Task<PaginaResponse<MovimientoResponse>> ObtenerMovimientosAsync(
		int usuarioId, PaginacionRequest paginacion)
	{
		var billeteraId = await _contexto.Billeteras
			.Where(b => b.UsuarioId == usuarioId)
			.Select(b => (int?)b.Id)
			.FirstOrDefaultAsync()
			?? throw new RecursoNoEncontradoException(SinBilletera);

		var movimientos = _contexto.AsientosLedger.Where(a => a.BilleteraId == billeteraId);
		var total = await movimientos.CountAsync();
		var asientos = await movimientos
			.AsNoTracking()
			.Include(a => a.Subasta)
			.OrderByDescending(a => a.Fecha)
			.ThenByDescending(a => a.Id)
			.Skip((paginacion.Pagina - 1) * paginacion.Tamano)
			.Take(paginacion.Tamano)
			.ToListAsync();

		return PaginaResponse.Crear(
			asientos.Select(MovimientoResponse.Desde).ToList(),
			paginacion.Pagina, paginacion.Tamano, total);
	}
}

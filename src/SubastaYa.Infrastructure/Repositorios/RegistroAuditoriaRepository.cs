using Microsoft.EntityFrameworkCore;
using SubastaYa.Domain.Entidades;
using SubastaYa.Domain.Repositorios;
using SubastaYa.Infrastructure.Persistencia;

namespace SubastaYa.Infrastructure.Repositorios;

public class RegistroAuditoriaRepository : IRegistroAuditoriaRepository
{
	private readonly AppDbContext _contexto;

	public RegistroAuditoriaRepository(AppDbContext contexto)
	{
		_contexto = contexto;
	}

	public async Task AgregarAsync(RegistroAuditoria registro)
	{
		_contexto.RegistrosAuditoria.Add(registro);
		await _contexto.SaveChangesAsync();
	}
}

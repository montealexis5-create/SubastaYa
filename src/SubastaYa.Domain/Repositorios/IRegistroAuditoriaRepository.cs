using SubastaYa.Domain.Entidades;

namespace SubastaYa.Domain.Repositorios;

public interface IRegistroAuditoriaRepository
{
	Task AgregarAsync(RegistroAuditoria registro);
}

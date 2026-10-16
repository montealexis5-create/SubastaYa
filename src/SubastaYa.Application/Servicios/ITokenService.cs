using SubastaYa.Domain.Entidades;

namespace SubastaYa.Application.Servicios;

public interface ITokenService
{
	(string Token, DateTime ExpiraEn) GenerarToken(Usuario usuario);
}

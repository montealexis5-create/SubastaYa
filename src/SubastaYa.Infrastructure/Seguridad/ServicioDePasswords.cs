using SubastaYa.Application.Servicios;

namespace SubastaYa.Infrastructure.Seguridad;

public class ServicioDePasswords : IServicioDePasswords
{
	private const int FactorDeCosto = 11;

	private static readonly string HashParaDescartar =
		BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString(), FactorDeCosto);

	public string HashDeDescarte => HashParaDescartar;

	public string Hashear(string password) => BCrypt.Net.BCrypt.HashPassword(password, FactorDeCosto);

	public bool Verificar(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);
}

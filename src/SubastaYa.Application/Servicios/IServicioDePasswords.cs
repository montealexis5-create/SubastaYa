namespace SubastaYa.Application.Servicios;

public interface IServicioDePasswords
{
	string HashDeDescarte { get; }

	string Hashear(string password);

	bool Verificar(string password, string hash);
}

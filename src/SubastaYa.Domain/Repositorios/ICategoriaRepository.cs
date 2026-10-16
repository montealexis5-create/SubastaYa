namespace SubastaYa.Domain.Repositorios;

public interface ICategoriaRepository
{
	Task<bool> ExisteAsync(int id);
}

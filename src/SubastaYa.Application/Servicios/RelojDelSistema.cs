namespace SubastaYa.Application.Servicios;

public class RelojDelSistema : IReloj
{
	public DateTime AhoraUtc => DateTime.UtcNow;
}

using Microsoft.Extensions.DependencyInjection;
using SubastaYa.Application.Servicios;
using SubastaYa.Domain.Entidades;
using SubastaYa.Domain.Enums;
using SubastaYa.Infrastructure.Persistencia;

namespace SubastaYa.Infrastructure.Seed;

public static class DatosSemilla
{
	private const string PasswordDePrueba = "Test1234";
	private const string DescripcionCargaInicial = "Carga inicial de saldo";
	private const int VersionInicial = 1;

	public static void AplicarDatosSemilla(this IServiceProvider serviceProvider)
	{
		using var scope = serviceProvider.CreateScope();
		var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();

		if (contexto.Usuarios.Any())
		{
			return;
		}

		var ahora = DateTime.UtcNow;

		var categorias = CargarCategorias(contexto);
		var usuarios = CargarUsuarios(contexto, ahora);
		var billeteras = CargarBilleteras(contexto, usuarios);
		var subastas = CargarSubastas(contexto, usuarios, categorias, ahora);
		var pujas = CargarPujas(contexto, subastas, usuarios, ahora);

		CargarLedger(contexto, billeteras, subastas, pujas, ahora);
	}

	private static CategoriasSemilla CargarCategorias(AppDbContext contexto)
	{
		var categorias = new CategoriasSemilla(
			new Categoria { Nombre = "Tecnología" },
			new Categoria { Nombre = "Indumentaria" },
			new Categoria { Nombre = "Vehículos" },
			new Categoria { Nombre = "Coleccionables" });

		contexto.Categorias.AddRange(
			categorias.Tecnologia, categorias.Indumentaria, categorias.Vehiculos, categorias.Coleccionables);
		contexto.SaveChanges();

		return categorias;
	}

	private static UsuariosSemilla CargarUsuarios(AppDbContext contexto, DateTime ahora)
	{
		var usuarios = new UsuariosSemilla(
			NuevoUsuario("vendedor@test.com", "Vendedor", "vendedor", ahora),
			NuevoUsuario("comprador1@test.com", "Comprador Uno", "comprador1", ahora),
			NuevoUsuario("comprador2@test.com", "Comprador Dos", "comprador2", ahora),
			NuevoUsuario("comprador3@test.com", "Comprador Tres", "comprador3", ahora),
			NuevoUsuario("sinfondos@test.com", "Sin Fondos", "sinfondos", ahora));

		contexto.Usuarios.AddRange(
			usuarios.Vendedor, usuarios.Comprador1, usuarios.Comprador2, usuarios.Comprador3, usuarios.SinFondos);
		contexto.SaveChanges();

		return usuarios;
	}

	private static BilleterasSemilla CargarBilleteras(AppDbContext contexto, UsuariosSemilla usuarios)
	{
		var billeteras = new BilleterasSemilla(
			Billetera.Abrir(0m, 0m, usuarios.Vendedor.Id),
			Billetera.Abrir(150000m, 45000m, usuarios.Comprador1.Id),
			Billetera.Abrir(200000m, 0m, usuarios.Comprador2.Id),
			Billetera.Abrir(50000m, 5500m, usuarios.Comprador3.Id),
			Billetera.Abrir(500m, 0m, usuarios.SinFondos.Id));

		contexto.Billeteras.AddRange(
			billeteras.Vendedor, billeteras.Comprador1, billeteras.Comprador2,
			billeteras.Comprador3, billeteras.SinFondos);
		contexto.SaveChanges();

		return billeteras;
	}

	private static SubastasSemilla CargarSubastas(
		AppDbContext contexto, UsuariosSemilla usuarios, CategoriasSemilla categorias, DateTime ahora)
	{
		var vendedorId = usuarios.Vendedor.Id;

		var subastas = new SubastasSemilla(
			NuevaSubasta(
				vendedorId, categorias.Tecnologia.Id,
				"Notebook Gamer usada", "Notebook gamer, poco uso, con cargador original.", "img/notebook.jpg",
				40000m, 1000m, 45000m, usuarios.Comprador1.Id,
				ahora.AddHours(-1), ahora.AddMinutes(25), EstadoSubasta.Activa, ahora.AddHours(-1)),
			NuevaSubasta(
				vendedorId, categorias.Indumentaria.Id,
				"Campera de cuero", "Campera de cuero negra, talle M, poco uso.", "img/campera.jpg",
				20000m, 500m, 20000m, null,
				ahora.AddMinutes(-10), ahora.AddSeconds(90), EstadoSubasta.Activa, ahora.AddMinutes(-10)),
			NuevaSubasta(
				vendedorId, categorias.Vehiculos.Id,
				"Bicicleta rodado 29", "Bicicleta de montaña, rodado 29, poco uso.", "img/bicicleta.jpg",
				60000m, 2000m, 60000m, null,
				ahora.AddHours(24), ahora.AddHours(25), EstadoSubasta.Programada, ahora),
			NuevaSubasta(
				vendedorId, categorias.Coleccionables.Id,
				"Álbum de figuritas completo", "Álbum antiguo completo, edición especial.", "img/album.jpg",
				5000m, 200m, 5500m, usuarios.Comprador3.Id,
				ahora.AddHours(-2), ahora.AddMinutes(-5), EstadoSubasta.Activa, ahora.AddHours(-2)),
			NuevaSubasta(
				vendedorId, categorias.Tecnologia.Id,
				"Router viejo sin uso", "Router en caja, nunca usado.", "img/router.jpg",
				3000m, 100m, 3000m, null,
				ahora.AddHours(-3), ahora.AddMinutes(-30), EstadoSubasta.Activa, ahora.AddHours(-3)));

		contexto.Subastas.AddRange(
			subastas.ActivaEstandar, subastas.ActivaCritica, subastas.Programada,
			subastas.VencidaConGanador, subastas.VencidaDesierta);
		contexto.SaveChanges();

		return subastas;
	}

	private static PujasSemilla CargarPujas(
		AppDbContext contexto, SubastasSemilla subastas, UsuariosSemilla usuarios, DateTime ahora)
	{
		var pujas = new PujasSemilla(
			NuevaPuja(subastas.ActivaEstandar.Id, usuarios.Comprador2.Id, 42000m, ahora.AddMinutes(-20)),
			NuevaPuja(subastas.ActivaEstandar.Id, usuarios.Comprador1.Id, 45000m, ahora.AddMinutes(-10)),
			NuevaPuja(subastas.VencidaConGanador.Id, usuarios.Comprador3.Id, 5500m, ahora.AddHours(-1)));

		contexto.Pujas.AddRange(pujas.PreviaComprador2, pujas.LiderComprador1, pujas.GanadoraVencida);

		return pujas;
	}

	private static void CargarLedger(
		AppDbContext contexto, BilleterasSemilla billeteras, SubastasSemilla subastas, PujasSemilla pujas,
		DateTime ahora)
	{
		var cargaInicial = ahora.AddDays(-1);

		contexto.AsientosLedger.AddRange(
			AsientoLedger.Nuevo(
				billeteras.Comprador1.Id, TipoAsiento.Deposito, 150000m, DescripcionCargaInicial, cargaInicial),
			AsientoLedger.Nuevo(
				billeteras.Comprador1.Id, TipoAsiento.Retencion, 45000m, DescripcionesDeAsiento.Retencion,
				ahora.AddMinutes(-10), subastas.ActivaEstandar.Id),
			AsientoLedger.Nuevo(
				billeteras.Comprador2.Id, TipoAsiento.Deposito, 200000m, DescripcionCargaInicial, cargaInicial),
			AsientoLedger.Nuevo(
				billeteras.Comprador2.Id, TipoAsiento.Retencion, 42000m, DescripcionesDeAsiento.Retencion,
				pujas.PreviaComprador2.FechaPuja, subastas.ActivaEstandar.Id),
			AsientoLedger.Nuevo(
				billeteras.Comprador2.Id, TipoAsiento.Liberacion, 42000m, DescripcionesDeAsiento.Liberacion,
				pujas.LiderComprador1.FechaPuja, subastas.ActivaEstandar.Id),
			AsientoLedger.Nuevo(
				billeteras.Comprador3.Id, TipoAsiento.Deposito, 50000m, DescripcionCargaInicial, cargaInicial),
			AsientoLedger.Nuevo(
				billeteras.Comprador3.Id, TipoAsiento.Retencion, 5500m, DescripcionesDeAsiento.Retencion,
				ahora.AddHours(-1), subastas.VencidaConGanador.Id),
			AsientoLedger.Nuevo(
				billeteras.SinFondos.Id, TipoAsiento.Deposito, 500m, DescripcionCargaInicial, cargaInicial));

		contexto.SaveChanges();
	}

	private static Usuario NuevoUsuario(string email, string nombre, string seudonimo, DateTime fechaRegistro)
	{
		return new Usuario
		{
			Email = email,
			PasswordHash = BCrypt.Net.BCrypt.HashPassword(PasswordDePrueba),
			Nombre = nombre,
			Seudonimo = seudonimo,
			FechaRegistro = fechaRegistro
		};
	}

	private static Subasta NuevaSubasta(
		int vendedorId, int categoriaId, string titulo, string descripcion, string urlImagen,
		decimal precioBase, decimal incrementoMinimo, decimal pujaActual, int? liderId,
		DateTime fechaInicio, DateTime fechaFin, EstadoSubasta estado, DateTime fechaCreacion)
	{
		return new Subasta
		{
			VendedorId = vendedorId,
			CategoriaId = categoriaId,
			Titulo = titulo,
			Descripcion = descripcion,
			UrlImagen = urlImagen,
			PrecioBase = precioBase,
			IncrementoMinimo = incrementoMinimo,
			PujaActual = pujaActual,
			LiderId = liderId,
			FechaInicio = fechaInicio,
			FechaFin = fechaFin,
			Estado = estado,
			Version = VersionInicial,
			FechaCreacion = fechaCreacion
		};
	}

	private static Puja NuevaPuja(int subastaId, int compradorId, decimal monto, DateTime fechaPuja)
	{
		return new Puja
		{
			SubastaId = subastaId,
			CompradorId = compradorId,
			Monto = monto,
			FechaPuja = fechaPuja
		};
	}

	private record CategoriasSemilla(
		Categoria Tecnologia, Categoria Indumentaria, Categoria Vehiculos, Categoria Coleccionables);

	private record UsuariosSemilla(
		Usuario Vendedor, Usuario Comprador1, Usuario Comprador2, Usuario Comprador3, Usuario SinFondos);

	private record BilleterasSemilla(
		Billetera Vendedor, Billetera Comprador1, Billetera Comprador2, Billetera Comprador3, Billetera SinFondos);

	private record SubastasSemilla(
		Subasta ActivaEstandar, Subasta ActivaCritica, Subasta Programada,
		Subasta VencidaConGanador, Subasta VencidaDesierta);

	private record PujasSemilla(Puja PreviaComprador2, Puja LiderComprador1, Puja GanadoraVencida);
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubastaYa.Domain.Entidades;

namespace SubastaYa.Infrastructure.Configuraciones;

public class SubastaConfiguration : IEntityTypeConfiguration<Subasta>
{
	public void Configure(EntityTypeBuilder<Subasta> builder)
	{
		builder.Property(s => s.Titulo)
			.HasMaxLength(150);

		builder.Property(s => s.Descripcion)
			.HasMaxLength(2000);

		builder.Property(s => s.UrlImagen)
			.HasMaxLength(500);

		builder.Property(s => s.PrecioBase)
			.HasColumnType("numeric(18,2)");

		builder.Property(s => s.IncrementoMinimo)
			.HasColumnType("numeric(18,2)");

		builder.Property(s => s.PujaActual)
			.HasColumnType("numeric(18,2)");

		builder.Property(s => s.Estado)
			.HasConversion<string>()
			.HasMaxLength(20);

		builder.Property(s => s.Version)
			.IsConcurrencyToken();

		builder.ToTable(t =>
		{
			t.HasCheckConstraint("CK_Subasta_PrecioBasePositivo", "\"PrecioBase\" > 0");
			t.HasCheckConstraint("CK_Subasta_IncrementoMinimoPositivo", "\"IncrementoMinimo\" > 0");
			t.HasCheckConstraint("CK_Subasta_FechaFinPosterior", "\"FechaFin\" > \"FechaInicio\"");
		});

		builder.HasIndex(s => new { s.Estado, s.FechaFin });

		builder.HasOne(s => s.Vendedor)
			.WithMany()
			.HasForeignKey(s => s.VendedorId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(s => s.Categoria)
			.WithMany()
			.HasForeignKey(s => s.CategoriaId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(s => s.Lider)
			.WithMany()
			.HasForeignKey(s => s.LiderId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}

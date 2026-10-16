using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SubastaYa.Domain.Entidades;

namespace SubastaYa.Infrastructure.Configuraciones;

public class AsientoLedgerConfiguration : IEntityTypeConfiguration<AsientoLedger>
{
	public void Configure(EntityTypeBuilder<AsientoLedger> builder)
	{
		builder.Property(a => a.Tipo)
			.HasConversion<string>()
			.HasMaxLength(20);

		builder.Property(a => a.Monto)
			.HasColumnType("numeric(18,2)");

		builder.Property(a => a.Descripcion)
			.HasMaxLength(255);

		builder.ToTable(t =>
			t.HasCheckConstraint("CK_AsientoLedger_MontoPositivo", "\"Monto\" > 0"));

		builder.HasIndex(a => new { a.BilleteraId, a.Fecha })
			.IsDescending(false, true);

		builder.HasOne(a => a.Billetera)
			.WithMany()
			.HasForeignKey(a => a.BilleteraId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.HasOne(a => a.Subasta)
			.WithMany()
			.HasForeignKey(a => a.SubastaId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}

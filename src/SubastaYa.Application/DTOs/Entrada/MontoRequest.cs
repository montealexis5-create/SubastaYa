using System.ComponentModel.DataAnnotations;

namespace SubastaYa.Application.DTOs.Entrada;

public abstract class MontoRequest : IValidatableObject
{
	[Range(typeof(decimal), "0.01", "1000000000",
		ParseLimitsInInvariantCulture = true,
		ErrorMessage = "El monto debe ser mayor a cero y no superar $1.000.000.000.")]
	public decimal Monto { get; set; }

	public IEnumerable<ValidationResult> Validate(ValidationContext contexto)
	{
		if (ValidacionDeMontos.TieneDemasiadosDecimales(Monto))
		{
			yield return new ValidationResult(
				"El monto no puede tener más de dos decimales.",
				new[] { nameof(Monto) });
		}
	}
}

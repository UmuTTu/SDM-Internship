namespace DemandTrack.Api.DTOS;
using System.ComponentModel.DataAnnotations;

public class Validate_Date : ValidationAttribute
{
    protected override/*override validation result class attribute*/ ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not DateOnly Date)
            return new ValidationResult("Girilen geçerli tarih formatında olmalıdır, Format: YYYY-MM-DD .");

        DateOnly Earliest = new DateOnly(2020, 1, 1);
        DateOnly Today = DateOnly.FromDateTime(DateTime.Now);

        if (Date < Earliest)
            return new ValidationResult("Oluşturma tarihi 2020'den önce olamaz.");

        else if (Date > Today)
            return new ValidationResult("Oluşturma tarihi bugünden sonra olamaz.");

        return ValidationResult.Success;
    }
}

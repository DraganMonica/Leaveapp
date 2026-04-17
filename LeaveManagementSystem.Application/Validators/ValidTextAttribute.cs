using System.Text.RegularExpressions;

namespace LeaveManagementSystem.Application.Validators;

public class ValidTextAttribute : ValidationAttribute
{
    public override bool IsValid(object? value)
    {
        if (value == null) return false;

        var text = value.ToString()!.Trim();

        // trebuie sa contina cel putin o litera(inclusiv diacritice)
        if (!Regex.IsMatch(text, @"\p{L}"))
            return false;

        // NU permite mai mult de 2 caractere identice consecutive
        if (Regex.IsMatch(text, @"(.)\1{2,}"))
            return false;

        return true;
    }
}


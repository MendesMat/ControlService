using System.Text.RegularExpressions;

namespace ControlService.Domain.SeedWork;

public static class CpfCnpjValidator
{
    public static bool IsCpf(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;

        var numbers = Regex.Replace(value, "[^0-9]", "");
        if (numbers.Length != 11) return false;

        if (new string(numbers[0], 11) == numbers) return false;

        var weights1 = new[] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        var weights2 = new[] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

        return ValidateCheckDigits(numbers, weights1, weights2);
    }

    public static bool IsCnpj(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;

        // Mantém letras e números (Suporte Alfanumérico 2026)
        var cleanValue = Regex.Replace(value, "[^0-9a-zA-Z]", "").ToUpper();
        if (cleanValue.Length != 14) return false;

        // Pesos conforme regra da Receita Federal
        var weights1 = new[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        var weights2 = new[] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        return ValidateCheckDigits(cleanValue, weights1, weights2);
    }

    private static bool ValidateCheckDigits(string value, int[] weights1, int[] weights2)
    {
        var firstDigit = CalculateDigit(value.Substring(0, weights1.Length), weights1);
        if (GetValue(value[weights1.Length]) != firstDigit) return false;

        var secondDigit = CalculateDigit(value.Substring(0, weights2.Length), weights2);
        if (GetValue(value[weights2.Length]) != secondDigit) return false;

        return true;
    }

    private static int CalculateDigit(string baseValue, int[] weights)
    {
        var sum = 0;
        for (int i = 0; i < weights.Length; i++)
        {
            sum += GetValue(baseValue[i]) * weights[i];
        }

        var remainder = sum % 11;
        return remainder < 2 ? 0 : 11 - remainder;
    }

    private static int GetValue(char c)
    {
        // Conforme Nota Técnica 49/2024: Valor = ASCII - 48
        return (int)c - 48;
    }
}

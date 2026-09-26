using System.Globalization;
using System.Text;

namespace ControlService.Domain.Common;

public static class TextNormalization
{
    public static string Normalize(string text)
    {
        var trimmedLowercase = text.Trim().ToLowerInvariant();
        var decomposed = trimmedLowercase.Normalize(NormalizationForm.FormD);

        var withoutAccents = new StringBuilder(decomposed.Length);
        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                withoutAccents.Append(character);
            }
        }

        return withoutAccents.ToString().Normalize(NormalizationForm.FormC);
    }
}

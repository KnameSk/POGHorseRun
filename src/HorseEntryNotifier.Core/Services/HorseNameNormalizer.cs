using System.Globalization;
using System.Text;

namespace HorseEntryNotifier.Core.Services;

public sealed class HorseNameNormalizer
{
    public string Normalize(string horseName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(horseName);

        var normalized = horseName.Normalize(NormalizationForm.FormKC);
        var builder = new StringBuilder(normalized.Length);

        foreach (var rune in normalized.EnumerateRunes())
        {
            if (!Rune.IsWhiteSpace(rune) &&
                Rune.GetUnicodeCategory(rune) is not UnicodeCategory.Format)
            {
                builder.Append(rune.ToString());
            }
        }

        return builder.ToString().ToUpperInvariant();
    }
}

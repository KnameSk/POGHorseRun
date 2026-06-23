using HorseEntryNotifier.Core.Services;

namespace HorseEntryNotifier.Tests;

public sealed class HorseNameNormalizerTests
{
    [Theory]
    [InlineData(" ジェット シェヴロン ", "ジェットシェヴロン")]
    [InlineData("ジェット　シェヴロン", "ジェットシェヴロン")]
    [InlineData("Ａｂｃ horse", "ABCHORSE")]
    public void Normalize_AbsorbsWidthWhitespaceAndCase(string value, string expected)
    {
        var normalizer = new HorseNameNormalizer();

        Assert.Equal(expected, normalizer.Normalize(value));
    }
}

using ControlService.Domain.Common;

namespace ControlService.Domain.Tests.Common;

public class TextNormalizationTests
{
    [Fact]
    public void Normalize_trims_leading_and_trailing_spaces() // CNV-03
    {
        var normalized = TextNormalization.Normalize("  ana souza  ");

        normalized.ShouldBe("ana souza");
    }

    [Fact]
    public void Normalize_lowercases() // CNV-09
    {
        var normalized = TextNormalization.Normalize("ANA SOUZA");

        normalized.ShouldBe("ana souza");
    }

    [Fact]
    public void Normalize_removes_accents() // CNV-09
    {
        var normalized = TextNormalization.Normalize("Ána Sôuza");

        normalized.ShouldBe("ana souza");
    }
}

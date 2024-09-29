using BaroquenMelody.Library.Domain;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Phrasing;

[ExcludeFromCodeCoverage]
internal sealed class RepeatedPhrase
{
    public int RepetitionCount { get; set; }

    public required List<Measure> Phrase { get; init; }
}

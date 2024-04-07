using BaroquenMelody.Library.Compositions.Domain;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Compositions.Phrasing;

[ExcludeFromCodeCoverage]
internal sealed class RepeatedPhrase
{
    public int RepetitionCount { get; set; }

    public required IList<Measure> Phrase { get; init; }

    public Guid Id { get; } = Guid.NewGuid();
}

using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Selection.Strategies;

internal sealed class CleanLowerNote : IOrnamentationCleaningSelectorStrategy
{
    public BaroquenNote? Select(BaroquenNote primaryNote, BaroquenNote secondaryNote) => (primaryNote, secondaryNote) switch
    {
        ({ } primary, { } secondary) when primary.NoteNumber > secondary.NoteNumber => secondary,
        ({ } primary, { } secondary) when primary.NoteNumber < secondary.NoteNumber => primary,
        _ => null
    };
}

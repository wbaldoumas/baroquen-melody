using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Ornamentation.Cleaning.Engine.Selection.Strategies;

internal sealed class CleanLowerNote : IOrnamentationCleaningSelectorStrategy
{
    public BaroquenNote? Select(BaroquenNote primaryNote, BaroquenNote secondaryNote) => (primaryNote, secondaryNote) switch
    {
        ({ } primary, { } secondary) when primary.NoteNumber > secondary.NoteNumber => secondary,
        ({ } primary, { } secondary) when primary.NoteNumber < secondary.NoteNumber => primary,
        _ => null
    };
}

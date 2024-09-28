using BaroquenMelody.Library.Compositions.MusicTheory;
using BaroquenMelody.Library.Compositions.Ornamentation.Enums;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Selection;

internal sealed class NoteIndexPairSelector(INoteOnsetCalculator noteOnsetCalculator) : INoteIndexPairSelector
{
    public IEnumerable<NoteIndexPair> Select(OrnamentationType primaryOrnamentationType, OrnamentationType secondaryOrnamentationType)
    {
        var primaryOrnamentationNoteOnsets = noteOnsetCalculator.CalculateNoteOnsets(primaryOrnamentationType);
        var secondaryOrnamentationNoteOnsets = noteOnsetCalculator.CalculateNoteOnsets(secondaryOrnamentationType);

        if (primaryOrnamentationNoteOnsets.Length != secondaryOrnamentationNoteOnsets.Length)
        {
            throw new InvalidOperationException("The note onsets for the primary and secondary ornamentation types are not the same length.");
        }

        var noteIndexPairs = new List<NoteIndexPair>();

        var primaryOrnamentationCursor = 0;
        var secondaryOrnamentationCursor = 0;

        for (var i = 1; i < primaryOrnamentationNoteOnsets.Length; i++)
        {
            var primaryOrnamentationNoteOnset = primaryOrnamentationNoteOnsets[i];
            var secondaryOrnamentationNoteOnset = secondaryOrnamentationNoteOnsets[i];

            if (primaryOrnamentationNoteOnset && secondaryOrnamentationNoteOnset)
            {
                noteIndexPairs.Add(new NoteIndexPair(primaryOrnamentationCursor, secondaryOrnamentationCursor, i));

                primaryOrnamentationCursor++;
                secondaryOrnamentationCursor++;
            }
            else
            {
                if (primaryOrnamentationNoteOnset)
                {
                    primaryOrnamentationCursor++;
                }

                if (secondaryOrnamentationNoteOnset)
                {
                    secondaryOrnamentationCursor++;
                }
            }
        }

        return noteIndexPairs;
    }
}

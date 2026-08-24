using Atrea.PolicyEngine.Policies.Output;
using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Ornamentation.Cleaning;

namespace BaroquenMelody.Library.Ornamentation.Engine.Policies.Output;

/// <summary>
///     Identifies ornamentations that conflict with each other and removes them, handing each (note, other note)
///     pair of the beat to the ornamentation cleaner.
/// </summary>
internal sealed class CleanConflictingOrnamentations(IProcessor<OrnamentationCleaningItem> ornamentationCleaner) : IOutputPolicy<OrnamentationItem>
{
    public void Apply(OrnamentationItem item)
    {
        var instruments = item.CurrentBeat.Chord.Notes.Select(note => note.Instrument).ToHashSet();
        var processedInstrumentCombinations = new HashSet<(Instrument, Instrument)>();

        foreach (var otherInstrument in instruments)
        {
            if (item.Instrument == otherInstrument || processedInstrumentCombinations.Contains((item.Instrument, otherInstrument)))
            {
                continue;
            }

            var note = item.CurrentBeat[item.Instrument];
            var otherNote = item.CurrentBeat[otherInstrument];

            ornamentationCleaner.Process(new OrnamentationCleaningItem(note, otherNote));

            processedInstrumentCombinations.Add((item.Instrument, otherInstrument));
        }
    }
}

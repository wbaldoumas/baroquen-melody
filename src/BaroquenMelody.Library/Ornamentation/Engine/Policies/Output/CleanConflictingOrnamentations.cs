using Atrea.PolicyEngine;
using Atrea.PolicyEngine.Policies.Output;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Ornamentation.Cleaning;
using LazyCart;

namespace BaroquenMelody.Library.Ornamentation.Engine.Policies.Output;

/// <summary>
///     Identifies ornamentations that conflict with each other and removes them.
/// </summary>
internal sealed class CleanConflictingOrnamentations(IPolicyEngine<OrnamentationCleaningItem> ornamentationCleaningEngine) : IOutputPolicy<OrnamentationItem>
{
    public void Apply(OrnamentationItem item)
    {
        var instruments = item.CurrentBeat.Chord.Notes.Select(note => note.Instrument).ToList();
        var instrumentCombinations = new LazyCartesianProduct<Instrument, Instrument>(instruments, instruments);
        var processedInstrumentCombinations = new HashSet<(Instrument, Instrument)>();

        for (var i = 0; i < instrumentCombinations.Size; ++i)
        {
            var (instrumentA, instrumentB) = instrumentCombinations[i];

            if (instrumentA == instrumentB || processedInstrumentCombinations.Contains((instrumentA, instrumentB)))
            {
                continue;
            }

            var noteA = item.CurrentBeat[instrumentA];
            var noteB = item.CurrentBeat[instrumentB];

            ornamentationCleaningEngine.Process(new OrnamentationCleaningItem(noteA, noteB));

            processedInstrumentCombinations.Add((instrumentA, instrumentB));
            processedInstrumentCombinations.Add((instrumentB, instrumentA));
        }
    }
}

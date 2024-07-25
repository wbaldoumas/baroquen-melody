using Atrea.PolicyEngine;
using Atrea.PolicyEngine.Policies.Output;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Ornamentation.Cleaning;
using LazyCart;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies.Output;

/// <summary>
///     Identifies ornamentations that conflict with each other and removes them.
/// </summary>
internal sealed class CleanConflictingOrnamentations(IPolicyEngine<OrnamentationCleaningItem> ornamentationCleaningEngine) : IOutputPolicy<OrnamentationItem>
{
    public void Apply(OrnamentationItem item)
    {
        var voices = item.CurrentBeat.Chord.Notes.Select(note => note.Voice).ToList();
        var voiceCombinations = new LazyCartesianProduct<Voice, Voice>(voices, voices);
        var processedVoiceCombinations = new HashSet<(Voice, Voice)>();

        for (var i = 0; i < voiceCombinations.Size; ++i)
        {
            var (voiceA, voiceB) = voiceCombinations[i];

            if (voiceA == voiceB || processedVoiceCombinations.Contains((voiceA, voiceB)))
            {
                continue;
            }

            var noteA = item.CurrentBeat[voiceA];
            var noteB = item.CurrentBeat[voiceB];

            ornamentationCleaningEngine.Process(new OrnamentationCleaningItem(noteA, noteB));

            processedVoiceCombinations.Add((voiceA, voiceB));
            processedVoiceCombinations.Add((voiceB, voiceA));
        }
    }
}

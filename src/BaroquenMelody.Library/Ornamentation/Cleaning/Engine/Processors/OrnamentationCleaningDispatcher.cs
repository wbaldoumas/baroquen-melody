using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Ornamentation.Enums;
using System.Collections.Frozen;

namespace BaroquenMelody.Library.Ornamentation.Cleaning.Engine.Processors;

/// <summary>
///     Routes an ornamentation cleaning item straight to the cleaner registered for its
///     (<see cref="OrnamentationCleaningItem.Note"/>, <see cref="OrnamentationCleaningItem.OtherNote"/>) ornamentation
///     pair. Every pair has at most one cleaner, so a keyed lookup is behaviourally identical to gating each cleaner
///     behind its own exact-match input policy - without walking every pair's gate for every item. A pair with no
///     cleaner (an un-ornamented or sustain-stamped partner) is left untouched.
/// </summary>
internal sealed class OrnamentationCleaningDispatcher(
    FrozenDictionary<(OrnamentationType Note, OrnamentationType OtherNote), IProcessor<OrnamentationCleaningItem>> cleanersByOrnamentationPair
) : IProcessor<OrnamentationCleaningItem>
{
    public void Process(OrnamentationCleaningItem item)
    {
        if (cleanersByOrnamentationPair.TryGetValue((item.Note.OrnamentationType, item.OtherNote.OrnamentationType), out var cleaner))
        {
            cleaner.Process(item);
        }
    }
}

using BaroquenMelody.Library.Compositions.Domain;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning;

[ExcludeFromCodeCoverage(Justification = "Record with no logic.")]
internal sealed record OrnamentationCleaningItem(BaroquenNote Note, BaroquenNote OtherNote);

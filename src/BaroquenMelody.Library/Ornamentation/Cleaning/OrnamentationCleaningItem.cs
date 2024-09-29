using BaroquenMelody.Library.Domain;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Ornamentation.Cleaning;

[ExcludeFromCodeCoverage(Justification = "Record with no logic.")]
internal sealed record OrnamentationCleaningItem(BaroquenNote Note, BaroquenNote OtherNote);

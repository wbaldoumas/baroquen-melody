using BaroquenMelody.Library.Enums;
using System.Collections.Frozen;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.Library.Motifs;

/// <summary>
///     An immutable catalog of the theme's per-voice motifs, keyed by <see cref="Instrument"/>. A later step looks up
///     a voice's <see cref="AnchoredMotif"/>, transforms it, and substitutes the developed restatement for a verbatim one.
/// </summary>
internal sealed class MotifBank
{
    private readonly FrozenDictionary<Instrument, AnchoredMotif> _motifsByInstrument;

    public MotifBank(IReadOnlyDictionary<Instrument, AnchoredMotif> motifsByInstrument)
    {
        ArgumentNullException.ThrowIfNull(motifsByInstrument);

        _motifsByInstrument = motifsByInstrument.ToFrozenDictionary();
    }

    public IReadOnlyCollection<Instrument> Voices => _motifsByInstrument.Keys;

    public bool Contains(Instrument instrument) => _motifsByInstrument.ContainsKey(instrument);

    // Throws KeyNotFoundException if the instrument has no cataloged motif; callers without that guarantee use TryGet.
    public AnchoredMotif Get(Instrument instrument) => _motifsByInstrument[instrument];

    public bool TryGet(Instrument instrument, [MaybeNullWhen(false)] out AnchoredMotif anchoredMotif) =>
        _motifsByInstrument.TryGetValue(instrument, out anchoredMotif);
}

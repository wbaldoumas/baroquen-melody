using BaroquenMelody.Library.Compositions.Midi;

namespace BaroquenMelody.Library.Compositions.Composers;

internal sealed class BaroquenMelodyComposer(
    IComposer composer,
    IMidiGenerator midiGenerator
) : IBaroquenMelodyComposer
{
    public BaroquenMelody Compose(CancellationToken cancellationToken)
    {
        var composition = composer.Compose(cancellationToken);
        var midiFile = midiGenerator.Generate(composition);

        return new BaroquenMelody(midiFile);
    }
}

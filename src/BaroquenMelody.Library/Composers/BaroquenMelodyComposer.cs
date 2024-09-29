using BaroquenMelody.Library.Midi;

namespace BaroquenMelody.Library.Composers;

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

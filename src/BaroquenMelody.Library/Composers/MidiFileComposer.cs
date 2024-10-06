using BaroquenMelody.Library.Midi;

namespace BaroquenMelody.Library.Composers;

internal sealed class MidiFileComposer(
    IComposer composer,
    IMidiGenerator midiGenerator
) : IMidiFileComposer
{
    public MidiFileComposition Compose(CancellationToken cancellationToken)
    {
        var composition = composer.Compose(cancellationToken);
        var midiFile = midiGenerator.Generate(composition);

        return new MidiFileComposition(midiFile);
    }
}

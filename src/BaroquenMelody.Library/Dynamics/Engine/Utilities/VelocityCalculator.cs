using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using Melanchall.DryWetMidi.Common;

namespace BaroquenMelody.Library.Dynamics.Engine.Utilities;

internal sealed class VelocityCalculator : IVelocityCalculator
{
    public SevenBitNumber GetPrecedingVelocity(FixedSizeList<Beat> precedingBeats, Instrument instrument)
    {
        Validate(precedingBeats, instrument);

        var precedingNote = precedingBeats[^1][instrument];

        return precedingNote.HasOrnamentations ? precedingNote.Ornamentations[^1].Velocity : precedingNote.Velocity;
    }

    public IEnumerable<SevenBitNumber> GetPrecedingVelocities(FixedSizeList<Beat> precedingBeats, Instrument instrument, int count = 4)
    {
        Validate(precedingBeats, instrument);

        var test = precedingBeats.TakeLast(count).Reverse().ToList();

        return precedingBeats.TakeLast(count)
            .Reverse()
            .Select(beat =>
            {
                var note = beat[instrument];

                return note.Ornamentations
                    .Reverse()
                    .Select(velocity => velocity.Velocity)
                    .Prepend(note.Velocity)
                    .ToList();
            })
            .SelectMany(velocities => velocities);
    }

    private static void Validate(FixedSizeList<Beat> precedingBeats, Instrument instrument)
    {
        if (precedingBeats.Count == 0)
        {
            throw new ArgumentException("Preceding beats is empty.");
        }

        if (!precedingBeats[^1].ContainsInstrument(instrument))
        {
            throw new ArgumentException($"Preceding beats does not contain the given instrument: {instrument}.");
        }
    }
}

using Atrea.Utilities.Enums;
using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Midi.Enums;
using FluentAssertions;
using LazyCart;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Midi.Enums;

[TestFixture]
internal sealed class MidiInstrumentTypeTests
{
    [Test]
    public void HasFlag_returns_expected_values()
    {
        // arrange
        var midiInstrumentTypes = EnumUtils<MidiInstrumentType>
            .AsEnumerable()
            .Where(midiInstrumentType => midiInstrumentType != MidiInstrumentType.None && midiInstrumentType != MidiInstrumentType.All)
            .OrderBy(_ => ThreadLocalRandom.Next())
            .ToList();

        var midiInstrumentCombos = new LazyCartesianProduct<MidiInstrumentType, MidiInstrumentType, MidiInstrumentType>(
            midiInstrumentTypes,
            midiInstrumentTypes,
            midiInstrumentTypes
        );

        // act + assert
        for (var i = 0; i < midiInstrumentCombos.Size; i++)
        {
            var midiInstrumentCombo = midiInstrumentCombos[i];

            // act
            var bitFlag = midiInstrumentCombo.Item1 | midiInstrumentCombo.Item2 | midiInstrumentCombo.Item3;

            // assert the flag combo has the expected flags
            bitFlag.HasFlag(midiInstrumentCombo.Item1).Should().BeTrue();
            bitFlag.HasFlag(midiInstrumentCombo.Item2).Should().BeTrue();
            bitFlag.HasFlag(midiInstrumentCombo.Item3).Should().BeTrue();

            // assert the flag combo does not have any other flags
            foreach (var midiInstrumentType in midiInstrumentTypes.Where(midiInstrumentType =>
                         midiInstrumentType != midiInstrumentCombo.Item1 &&
                         midiInstrumentType != midiInstrumentCombo.Item2 &&
                         midiInstrumentType != midiInstrumentCombo.Item3))
            {
                bitFlag.HasFlag(midiInstrumentType).Should().BeFalse();
            }
        }
    }
}

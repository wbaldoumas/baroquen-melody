using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Output;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Ornamentation.Engine.Policies.Output;

[TestFixture]
internal sealed class LogAppliedOrnamentationTests
{
    [Test]
    [TestCase(OrnamentationType.PassingTone, Instrument.One)]
    [TestCase(OrnamentationType.Sustain, Instrument.Three)]
    public void Apply_LogsTheOrnamentationTypeNowOnTheInstrumentsNote(OrnamentationType ornamentationType, Instrument instrument)
    {
        // arrange - one policy instance serves every processor, so it reads the type off the note rather than carrying its own
        var logger = new CapturingLogger();
        var policy = new LogAppliedOrnamentation(logger);

        var note = new BaroquenNote(instrument, Notes.C4, MusicalTimeSpan.Half) { OrnamentationType = ornamentationType };
        var item = new OrnamentationItem(instrument, new FixedSizeList<Beat>(1), new Beat(new BaroquenChord([note])), null);

        // act
        policy.Apply(item);

        // assert
        logger.Messages.Should().ContainSingle().Which.Should().Be($"Ornamentation {ornamentationType} applied to instrument {instrument}.");
    }
}

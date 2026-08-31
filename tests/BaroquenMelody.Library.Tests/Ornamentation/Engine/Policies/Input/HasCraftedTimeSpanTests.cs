using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;
using BaroquenMelody.Library.Ornamentation.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Ornamentation.Engine.Policies.Input;

[TestFixture]
internal sealed class HasCraftedTimeSpanTests
{
    private static readonly CompositionConfiguration Configuration = TestCompositionConfigurations.Get(2);

    private HasCraftedTimeSpan _hasCraftedTimeSpan = null!;

    [SetUp]
    public void SetUp() => _hasCraftedTimeSpan = new HasCraftedTimeSpan(Configuration);

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void ShouldProcess_ShouldReturnExpectedResult(OrnamentationItem ornamentationItem, InputPolicyResult expectedResult)
    {
        // act
        var result = _hasCraftedTimeSpan.ShouldProcess(ornamentationItem);

        // assert
        result.Should().Be(expectedResult);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            yield return new TestCaseData(
                CreateItem(
                    new BaroquenNote(Instrument.One, Notes.C4, Configuration.DefaultNoteTimeSpan),
                    new BaroquenNote(Instrument.One, Notes.C4, Configuration.DefaultNoteTimeSpan)
                ),
                InputPolicyResult.Reject
            ).SetName("When both notes sit at the default time span, policy rejects.");

            yield return new TestCaseData(
                CreateItem(
                    new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Whole),
                    new BaroquenNote(Instrument.One, Notes.C4, Configuration.DefaultNoteTimeSpan)
                ),
                InputPolicyResult.Continue
            ).SetName("When the current note carries a crafted whole-note span, policy continues.");

            yield return new TestCaseData(
                CreateItem(
                    new BaroquenNote(Instrument.One, Notes.C4, Configuration.DefaultNoteTimeSpan),
                    new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Whole)
                ),
                InputPolicyResult.Continue
            ).SetName("When the next note carries a crafted whole-note span, policy continues.");

            yield return new TestCaseData(
                CreateItem(
                    new BaroquenNote(Instrument.One, Notes.C4, Configuration.DefaultNoteTimeSpan),
                    new BaroquenNote(Instrument.One, Notes.C4, Configuration.DefaultNoteTimeSpan + (Configuration.DefaultNoteTimeSpan / 2))
                    {
                        OrnamentationType = OrnamentationType.Suspension
                    }
                ),
                InputPolicyResult.Reject
            ).SetName("When the next note is a stamped suspension preparation, policy rejects so the tie can absorb it.");

            yield return new TestCaseData(
                CreateItem(
                    new BaroquenNote(Instrument.One, Notes.C4, Configuration.DefaultNoteTimeSpan),
                    new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Quarter)
                    {
                        OrnamentationType = OrnamentationType.NeighborTone
                    }
                ),
                InputPolicyResult.Reject
            ).SetName("When the next note is a figured principal, policy rejects so the tie can absorb it.");

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.One,
                    new FixedSizeList<Beat>(1),
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Whole)])),
                    null
                ),
                InputPolicyResult.Continue
            ).SetName("When the current note carries a crafted span and there is no next beat, policy continues.");

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.Two,
                    new FixedSizeList<Beat>(1),
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Whole)])),
                    new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Whole)]))
                ),
                InputPolicyResult.Reject
            ).SetName("When neither beat contains the instrument, policy rejects.");
        }
    }

    private static OrnamentationItem CreateItem(BaroquenNote currentNote, BaroquenNote nextNote) => new(
        Instrument.One,
        new FixedSizeList<Beat>(1),
        new Beat(new BaroquenChord([currentNote])),
        new Beat(new BaroquenChord([nextNote]))
    );
}

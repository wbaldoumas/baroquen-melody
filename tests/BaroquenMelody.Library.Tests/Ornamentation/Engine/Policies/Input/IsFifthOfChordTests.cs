using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Infrastructure.Collections;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Ornamentation.Engine.Policies.Input;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Ornamentation.Engine.Policies.Input;

[TestFixture]
internal sealed class IsFifthOfChordTests
{
    private IsFifthOfChord _isRootOfChord;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = TestCompositionConfigurations.GetCompositionConfiguration(3);

        var chordNumberIdentifier = new ChordNumberIdentifier(compositionConfiguration);

        _isRootOfChord = new IsFifthOfChord(chordNumberIdentifier, compositionConfiguration);
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void ShouldProcess_returns_expected_result(OrnamentationItem item, InputPolicyResult expectedResult)
    {
        // act
        var result = _isRootOfChord.ShouldProcess(item);

        // assert
        result.Should().Be(expectedResult);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            var sopranoC4 = new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Half);
            var altoE3 = new BaroquenNote(Instrument.Two, Notes.E3, MusicalTimeSpan.Half);
            var tenorG2 = new BaroquenNote(Instrument.Three, Notes.G2, MusicalTimeSpan.Half);

            var i = new BaroquenChord([sopranoC4, altoE3, tenorG2]);

            var sopranoD4 = new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Half);
            var altoF3 = new BaroquenNote(Instrument.Two, Notes.F3, MusicalTimeSpan.Half);
            var tenorA2 = new BaroquenNote(Instrument.Three, Notes.A2, MusicalTimeSpan.Half);

            var ii = new BaroquenChord([sopranoD4, altoF3, tenorA2]);

            var sopranoE4 = new BaroquenNote(Instrument.One, Notes.E4, MusicalTimeSpan.Half);
            var altoG3 = new BaroquenNote(Instrument.Two, Notes.G3, MusicalTimeSpan.Half);
            var tenorB2 = new BaroquenNote(Instrument.Three, Notes.B2, MusicalTimeSpan.Half);

            var iii = new BaroquenChord([sopranoE4, altoG3, tenorB2]);

            var sopranoF4 = new BaroquenNote(Instrument.One, Notes.F4, MusicalTimeSpan.Half);
            var altoA3 = new BaroquenNote(Instrument.Two, Notes.A3, MusicalTimeSpan.Half);
            var tenorC3 = new BaroquenNote(Instrument.Three, Notes.C3, MusicalTimeSpan.Half);

            var iv = new BaroquenChord([sopranoF4, altoA3, tenorC3]);

            var sopranoG4 = new BaroquenNote(Instrument.One, Notes.G4, MusicalTimeSpan.Half);
            var altoB3 = new BaroquenNote(Instrument.Two, Notes.B3, MusicalTimeSpan.Half);
            var tenorD3 = new BaroquenNote(Instrument.Three, Notes.D3, MusicalTimeSpan.Half);

            var v = new BaroquenChord([sopranoG4, altoB3, tenorD3]);

            var sopranoA4 = new BaroquenNote(Instrument.One, Notes.A4, MusicalTimeSpan.Half);
            var altoC4 = new BaroquenNote(Instrument.Two, Notes.C4, MusicalTimeSpan.Half);
            var tenorE3 = new BaroquenNote(Instrument.Three, Notes.E3, MusicalTimeSpan.Half);

            var vi = new BaroquenChord([sopranoA4, altoC4, tenorE3]);

            var sopranoB4 = new BaroquenNote(Instrument.One, Notes.B4, MusicalTimeSpan.Half);
            var altoD4 = new BaroquenNote(Instrument.Two, Notes.D4, MusicalTimeSpan.Half);
            var tenorF3 = new BaroquenNote(Instrument.Three, Notes.F3, MusicalTimeSpan.Half);

            var vii = new BaroquenChord([sopranoB4, altoD4, tenorF3]);

            var unknown = new BaroquenChord([sopranoC4, altoE3, tenorF3]);

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.One,
                    new FixedSizeList<Beat>(1),
                    new Beat(i),
                    null
                ),
                InputPolicyResult.Reject
            );

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.One,
                    new FixedSizeList<Beat>(1),
                    new Beat(ii),
                    null
                ),
                InputPolicyResult.Reject
            );

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.One,
                    new FixedSizeList<Beat>(1),
                    new Beat(iii),
                    null
                ),
                InputPolicyResult.Reject
            );

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.One,
                    new FixedSizeList<Beat>(1),
                    new Beat(iv),
                    null
                ),
                InputPolicyResult.Reject
            );

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.One,
                    new FixedSizeList<Beat>(1),
                    new Beat(v),
                    null
                ),
                InputPolicyResult.Reject
            );

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.One,
                    new FixedSizeList<Beat>(1),
                    new Beat(vi),
                    null
                ),
                InputPolicyResult.Reject
            );

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.One,
                    new FixedSizeList<Beat>(1),
                    new Beat(vii),
                    null
                ),
                InputPolicyResult.Reject
            );

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.Three,
                    new FixedSizeList<Beat>(1),
                    new Beat(unknown),
                    null
                ),
                InputPolicyResult.Reject
            );

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.Three,
                    new FixedSizeList<Beat>(1),
                    new Beat(i),
                    null
                ),
                InputPolicyResult.Continue
            );

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.Three,
                    new FixedSizeList<Beat>(1),
                    new Beat(ii),
                    null
                ),
                InputPolicyResult.Continue
            );

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.Three,
                    new FixedSizeList<Beat>(1),
                    new Beat(iii),
                    null
                ),
                InputPolicyResult.Continue
            );

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.Three,
                    new FixedSizeList<Beat>(1),
                    new Beat(iv),
                    null
                ),
                InputPolicyResult.Continue
            );

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.Three,
                    new FixedSizeList<Beat>(1),
                    new Beat(v),
                    null
                ),
                InputPolicyResult.Continue
            );

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.Three,
                    new FixedSizeList<Beat>(1),
                    new Beat(vi),
                    null
                ),
                InputPolicyResult.Continue
            );

            yield return new TestCaseData(
                new OrnamentationItem(
                    Instrument.Three,
                    new FixedSizeList<Beat>(1),
                    new Beat(vii),
                    null
                ),
                InputPolicyResult.Continue
            );
        }
    }
}

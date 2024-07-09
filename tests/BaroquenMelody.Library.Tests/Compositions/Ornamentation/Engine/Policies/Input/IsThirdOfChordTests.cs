using Atrea.PolicyEngine.Policies.Input;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.MusicTheory;
using BaroquenMelody.Library.Compositions.Ornamentation;
using BaroquenMelody.Library.Compositions.Ornamentation.Engine.Policies.Input;
using BaroquenMelody.Library.Infrastructure.Collections;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Ornamentation.Engine.Policies.Input;

[TestFixture]
internal sealed class IsThirdOfChordTests
{
    private IsThirdOfChord _isRootOfChord;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                new(Voice.Soprano, Notes.C3, Notes.G6),
                new(Voice.Alto, Notes.C2, Notes.G5),
                new(Voice.Tenor, Notes.C1, Notes.G4)
            },
            BaroquenScale.Parse("C Major"),
            Meter.FourFour,
            CompositionLength: 100
        );

        var chordNumberIdentifier = new ChordNumberIdentifier(compositionConfiguration);

        _isRootOfChord = new IsThirdOfChord(chordNumberIdentifier, compositionConfiguration);
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
            var sopranoC4 = new BaroquenNote(Voice.Soprano, Notes.C4);
            var altoE3 = new BaroquenNote(Voice.Alto, Notes.E3);
            var tenorG2 = new BaroquenNote(Voice.Tenor, Notes.G2);

            var i = new BaroquenChord([sopranoC4, altoE3, tenorG2]);

            var sopranoD4 = new BaroquenNote(Voice.Soprano, Notes.D4);
            var altoF3 = new BaroquenNote(Voice.Alto, Notes.F3);
            var tenorA2 = new BaroquenNote(Voice.Tenor, Notes.A2);

            var ii = new BaroquenChord([sopranoD4, altoF3, tenorA2]);

            var sopranoE4 = new BaroquenNote(Voice.Soprano, Notes.E4);
            var altoG3 = new BaroquenNote(Voice.Alto, Notes.G3);
            var tenorB2 = new BaroquenNote(Voice.Tenor, Notes.B2);

            var iii = new BaroquenChord([sopranoE4, altoG3, tenorB2]);

            var sopranoF4 = new BaroquenNote(Voice.Soprano, Notes.F4);
            var altoA3 = new BaroquenNote(Voice.Alto, Notes.A3);
            var tenorC3 = new BaroquenNote(Voice.Tenor, Notes.C3);

            var iv = new BaroquenChord([sopranoF4, altoA3, tenorC3]);

            var sopranoG4 = new BaroquenNote(Voice.Soprano, Notes.G4);
            var altoB3 = new BaroquenNote(Voice.Alto, Notes.B3);
            var tenorD3 = new BaroquenNote(Voice.Tenor, Notes.D3);

            var v = new BaroquenChord([sopranoG4, altoB3, tenorD3]);

            var sopranoA4 = new BaroquenNote(Voice.Soprano, Notes.A4);
            var altoC4 = new BaroquenNote(Voice.Alto, Notes.C4);
            var tenorE3 = new BaroquenNote(Voice.Tenor, Notes.E3);

            var vi = new BaroquenChord([sopranoA4, altoC4, tenorE3]);

            var sopranoB4 = new BaroquenNote(Voice.Soprano, Notes.B4);
            var altoD4 = new BaroquenNote(Voice.Alto, Notes.D4);
            var tenorF3 = new BaroquenNote(Voice.Tenor, Notes.F3);

            var vii = new BaroquenChord([sopranoB4, altoD4, tenorF3]);

            var unknown = new BaroquenChord([sopranoC4, altoE3, tenorF3]);

            yield return new TestCaseData(
                new OrnamentationItem(
                    Voice.Soprano,
                    new FixedSizeList<Beat>(1),
                    new Beat(i),
                    null
                ),
                InputPolicyResult.Reject
            );

            yield return new TestCaseData(
                new OrnamentationItem(
                    Voice.Soprano,
                    new FixedSizeList<Beat>(1),
                    new Beat(ii),
                    null
                ),
                InputPolicyResult.Reject
            );

            yield return new TestCaseData(
                new OrnamentationItem(
                    Voice.Soprano,
                    new FixedSizeList<Beat>(1),
                    new Beat(iii),
                    null
                ),
                InputPolicyResult.Reject
            );

            yield return new TestCaseData(
                new OrnamentationItem(
                    Voice.Soprano,
                    new FixedSizeList<Beat>(1),
                    new Beat(iv),
                    null
                ),
                InputPolicyResult.Reject
            );

            yield return new TestCaseData(
                new OrnamentationItem(
                    Voice.Soprano,
                    new FixedSizeList<Beat>(1),
                    new Beat(v),
                    null
                ),
                InputPolicyResult.Reject
            );

            yield return new TestCaseData(
                new OrnamentationItem(
                    Voice.Soprano,
                    new FixedSizeList<Beat>(1),
                    new Beat(vi),
                    null
                ),
                InputPolicyResult.Reject
            );

            yield return new TestCaseData(
                new OrnamentationItem(
                    Voice.Soprano,
                    new FixedSizeList<Beat>(1),
                    new Beat(vii),
                    null
                ),
                InputPolicyResult.Reject
            );

            yield return new TestCaseData(
                new OrnamentationItem(
                    Voice.Alto,
                    new FixedSizeList<Beat>(1),
                    new Beat(unknown),
                    null
                ),
                InputPolicyResult.Reject
            );

            yield return new TestCaseData(
                new OrnamentationItem(
                    Voice.Alto,
                    new FixedSizeList<Beat>(1),
                    new Beat(i),
                    null
                ),
                InputPolicyResult.Continue
            );

            yield return new TestCaseData(
                new OrnamentationItem(
                    Voice.Alto,
                    new FixedSizeList<Beat>(1),
                    new Beat(ii),
                    null
                ),
                InputPolicyResult.Continue
            );

            yield return new TestCaseData(
                new OrnamentationItem(
                    Voice.Alto,
                    new FixedSizeList<Beat>(1),
                    new Beat(iii),
                    null
                ),
                InputPolicyResult.Continue
            );

            yield return new TestCaseData(
                new OrnamentationItem(
                    Voice.Alto,
                    new FixedSizeList<Beat>(1),
                    new Beat(iv),
                    null
                ),
                InputPolicyResult.Continue
            );

            yield return new TestCaseData(
                new OrnamentationItem(
                    Voice.Alto,
                    new FixedSizeList<Beat>(1),
                    new Beat(v),
                    null
                ),
                InputPolicyResult.Continue
            );

            yield return new TestCaseData(
                new OrnamentationItem(
                    Voice.Alto,
                    new FixedSizeList<Beat>(1),
                    new Beat(vi),
                    null
                ),
                InputPolicyResult.Continue
            );

            yield return new TestCaseData(
                new OrnamentationItem(
                    Voice.Alto,
                    new FixedSizeList<Beat>(1),
                    new Beat(vii),
                    null
                ),
                InputPolicyResult.Continue
            );
        }
    }
}

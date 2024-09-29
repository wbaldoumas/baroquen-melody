using Atrea.Utilities.Enums;
using BaroquenMelody.Infrastructure.Collections.Extensions;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Rules;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Tests.Compositions.Rules;

[TestFixture]
internal sealed class FollowsStandardProgressionTests
{
    private FollowsStandardProgression _followsStandardProgression = null!;

    [SetUp]
    public void SetUp()
    {
        var compositionConfiguration = Configurations.GetCompositionConfiguration(3);

        _followsStandardProgression = new FollowsStandardProgression(compositionConfiguration);
    }

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Evaluate_ReturnsExpectedResult(IReadOnlyList<BaroquenChord> precedingChords, BaroquenChord nextChord, bool expectedResult) =>
        _followsStandardProgression.Evaluate(precedingChords, nextChord).Should().Be(expectedResult);

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            var i = new List<Note> { Notes.C4, Notes.E3, Notes.G2 };
            var ii = new List<Note> { Notes.D4, Notes.F3, Notes.A2 };
            var iii = new List<Note> { Notes.E4, Notes.G3, Notes.B2 };
            var iv = new List<Note> { Notes.F4, Notes.A3, Notes.C3 };
            var v = new List<Note> { Notes.G4, Notes.B3, Notes.D3 };
            var vi = new List<Note> { Notes.A4, Notes.C4, Notes.E3 };
            var vii = new List<Note> { Notes.B4, Notes.D4, Notes.F3 };

            var powerSetI = i.ToPowerSet().ToList();
            var powerSetII = ii.ToPowerSet().ToList();
            var powerSetIII = iii.ToPowerSet().ToList();
            var powerSetIV = iv.ToPowerSet().ToList();
            var powerSetV = v.ToPowerSet().ToList();
            var powerSetVI = vi.ToPowerSet().ToList();
            var powerSetVII = vii.ToPowerSet().ToList();

            var validPowerSetProgressions = new List<(List<HashSet<Note>>, List<HashSet<Note>>)>
            {
                (powerSetI, powerSetI),
                (powerSetI, powerSetII),
                (powerSetI, powerSetIII),
                (powerSetI, powerSetIV),
                (powerSetI, powerSetV),
                (powerSetI, powerSetVI),
                (powerSetI, powerSetVII),

                (powerSetII, powerSetI),
                (powerSetII, powerSetII),
                (powerSetII, powerSetIII),
                (powerSetII, powerSetV),
                (powerSetII, powerSetVI),
                (powerSetII, powerSetVII),

                (powerSetIII, powerSetII),
                (powerSetIII, powerSetIII),
                (powerSetIII, powerSetIV),
                (powerSetIII, powerSetVI),

                (powerSetIV, powerSetI),
                (powerSetIV, powerSetIII),
                (powerSetIV, powerSetIV),
                (powerSetIV, powerSetV),
                (powerSetIV, powerSetVII),

                (powerSetV, powerSetI),
                (powerSetV, powerSetV),
                (powerSetV, powerSetVI),

                (powerSetVI, powerSetII),
                (powerSetVI, powerSetIV),
                (powerSetVI, powerSetVI),

                (powerSetVII, powerSetI),
                (powerSetVII, powerSetIII),
                (powerSetVII, powerSetVI),
                (powerSetVII, powerSetVII)
            };

            var instruments = EnumUtils<Instrument>.AsEnumerable().ToArray();

            var invalidProgressions = new List<(List<Note>, List<Note>)>
            {
                (ii, iv),

                (iii, i),
                (iii, v),
                (iii, vii),

                (iv, ii),
                (iv, vi),

                (v, ii),
                (v, iv),
                (v, vii),

                (vi, i),
                (vi, iii),
                (vi, v),
                (vi, vii),

                (vii, ii),
                (vii, iv),
                (vii, v)
            };

            foreach (var (precedingPowerSet, nextPowerSet) in validPowerSetProgressions)
            {
                foreach (var precedingChord in precedingPowerSet.Where(subset => subset.Count > 0))
                {
                    var indexablePrecedingChord = precedingChord.ToList();

                    foreach (var nextChord in nextPowerSet.Where(subset => subset.Count > 0))
                    {
                        var indexableNextChord = nextChord.ToList();

                        yield return GenerateTestCase(indexablePrecedingChord, indexableNextChord, true);
                    }
                }
            }

            foreach (var (precedingChord, nextChord) in invalidProgressions)
            {
                yield return GenerateTestCase(precedingChord, nextChord, false);
            }

            yield break;

            TestCaseData GenerateTestCase(List<Note> precedingChord, List<Note> nextChord, bool isValid)
            {
                var precedingNotes = precedingChord.Select((precedingNote, precedingIndex) =>
                    new BaroquenNote(instruments[precedingIndex], precedingNote, MusicalTimeSpan.Quarter)
                ).ToList();

                var nextNotes = nextChord.Select((note, nextIndex) =>
                    new BaroquenNote(instruments[nextIndex], note, MusicalTimeSpan.Quarter)
                ).ToList();

                return new TestCaseData(
                    new List<BaroquenChord>
                    {
                        new(precedingNotes)
                    },
                    new BaroquenChord(nextNotes),
                    isValid
                );
            }
        }
    }
}

using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Ornamentation.Cleaning;
using BaroquenMelody.Library.Ornamentation.Cleaning.Engine.Selection;
using BaroquenMelody.Library.Ornamentation.Enums;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Ornamentation.Cleaning.Engine.Selection;

[TestFixture]
internal sealed class NotePairSelectorTests
{
    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void Selector_returns_expected_selection(
        IEnumerable<OrnamentationType> primaryNoteOrnamentationTypes,
        IEnumerable<OrnamentationType> secondaryNoteOrnamentationTypes,
        OrnamentationCleaningItem item,
        NotePair expectedSelection)
    {
        // arrange
        var selector = new NotePairSelector(primaryNoteOrnamentationTypes, secondaryNoteOrnamentationTypes);

        // act
        var selection = selector.Select(item);

        // assert
        selection.Should().BeEquivalentTo(expectedSelection);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            var ornamentationCleaningItem = new OrnamentationCleaningItem(
                new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Eighth)
                {
                    OrnamentationType = OrnamentationType.Run
                },
                new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Eighth)
                {
                    OrnamentationType = OrnamentationType.DoubleRun
                }
            );

            var targetOrnamentationTypesA = new List<OrnamentationType> { OrnamentationType.Run, OrnamentationType.DecorateInterval };
            var targetOrnamentationTypesB = new List<OrnamentationType> { OrnamentationType.DoubleRun, OrnamentationType.DecorateInterval };

            yield return new TestCaseData(
                targetOrnamentationTypesA,
                targetOrnamentationTypesB,
                ornamentationCleaningItem,
                new NotePair(
                    new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Eighth)
                    {
                        OrnamentationType = OrnamentationType.Run
                    },
                    new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Eighth)
                    {
                        OrnamentationType = OrnamentationType.DoubleRun
                    }
                )
            );

            yield return new TestCaseData(
                targetOrnamentationTypesB,
                targetOrnamentationTypesA,
                ornamentationCleaningItem,
                new NotePair(
                    new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Eighth)
                    {
                        OrnamentationType = OrnamentationType.DoubleRun
                    },
                    new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Eighth)
                    {
                        OrnamentationType = OrnamentationType.Run
                    }
                )
            );
        }
    }

    [Test]
    public void Selector_throws_when_ornamentation_types_do_not_match()
    {
        // arrange
        var selector = new NotePairSelector(
            new List<OrnamentationType> { OrnamentationType.Run },
            new List<OrnamentationType> { OrnamentationType.DoubleRun }
        );

        var ornamentationCleaningItem = new OrnamentationCleaningItem(
            new BaroquenNote(Instrument.One, Notes.C4, MusicalTimeSpan.Eighth)
            {
                OrnamentationType = OrnamentationType.Run
            },
            new BaroquenNote(Instrument.One, Notes.D4, MusicalTimeSpan.Eighth)
            {
                OrnamentationType = OrnamentationType.DecorateInterval
            }
        );

        // act
        var act = () => selector.Select(ornamentationCleaningItem);

        // assert
        act.Should().Throw<InvalidOperationException>();
    }
}

using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Contexts;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Contexts;

[TestFixture]
internal sealed class ChordContextGeneratorTests
{
    private IChordContextGenerator _chordContextGenerator = null!;

    [SetUp]
    public void SetUp() => _chordContextGenerator = new ChordContextGenerator();

    [Test]
    [TestCaseSource(nameof(TestCases))]
    public void WhenGenerateChordContextIsInvoked_ThenExpectedChordContextIsGenerated(
        ContextualizedChord previousChord,
        ContextualizedChord currentChord,
        ChordContext expectedChordContext)
    {
        // act + assert
        _chordContextGenerator.GenerateChordContext(previousChord, currentChord)
            .Should()
            .BeEquivalentTo(expectedChordContext);
    }

    private static IEnumerable<TestCaseData> TestCases
    {
        get
        {
            yield return new TestCaseData(
                new ContextualizedChord(
                    new HashSet<ContextualizedNote>
                    {
                        new(
                            Note.Get(NoteName.C, 4),
                            Voice.Soprano,
                            new NoteContext(Voice.Soprano, Note.Get(NoteName.C, 4), NoteMotion.Oblique, NoteSpan.None),
                            new NoteChoice(Voice.Soprano, NoteMotion.Oblique, 0)
                        ),
                        new(
                            Note.Get(NoteName.E, 3),
                            Voice.Alto,
                            new NoteContext(Voice.Alto, Note.Get(NoteName.E, 4), NoteMotion.Oblique, NoteSpan.None),
                            new NoteChoice(Voice.Alto, NoteMotion.Oblique, 0)
                        ),
                        new(
                            Note.Get(NoteName.G, 2),
                            Voice.Tenor,
                            new NoteContext(Voice.Tenor, Note.Get(NoteName.G, 4), NoteMotion.Oblique, NoteSpan.None),
                            new NoteChoice(Voice.Tenor, NoteMotion.Oblique, 0)
                        ),
                        new(
                            Note.Get(NoteName.C, 1),
                            Voice.Bass,
                            new NoteContext(Voice.Bass, Note.Get(NoteName.C, 4), NoteMotion.Oblique, NoteSpan.None),
                            new NoteChoice(Voice.Bass, NoteMotion.Oblique, 0)
                        )
                    },
                    new ChordContext([]),
                    new ChordChoice([])
                ),
                new ContextualizedChord(
                    new HashSet<ContextualizedNote>
                    {
                        new(
                            Note.Get(NoteName.C, 4),
                            Voice.Soprano,
                            new NoteContext(Voice.Soprano, Note.Get(NoteName.C, 4), NoteMotion.Oblique, NoteSpan.None),
                            new NoteChoice(Voice.Soprano, NoteMotion.Oblique, 0)
                        ),
                        new(
                            Note.Get(NoteName.F, 3),
                            Voice.Alto,
                            new NoteContext(Voice.Alto, Note.Get(NoteName.E, 4), NoteMotion.Oblique, NoteSpan.None),
                            new NoteChoice(Voice.Alto, NoteMotion.Oblique, 0)
                        ),
                        new(
                            Note.Get(NoteName.F, 2),
                            Voice.Tenor,
                            new NoteContext(Voice.Tenor, Note.Get(NoteName.G, 4), NoteMotion.Oblique, NoteSpan.None),
                            new NoteChoice(Voice.Tenor, NoteMotion.Oblique, 0)
                        ),
                        new(
                            Note.Get(NoteName.G, 1),
                            Voice.Bass,
                            new NoteContext(Voice.Bass, Note.Get(NoteName.C, 4), NoteMotion.Oblique, NoteSpan.None),
                            new NoteChoice(Voice.Bass, NoteMotion.Oblique, 0)
                        )
                    },
                    new ChordContext([]),
                    new ChordChoice([])
                ),
                new ChordContext(
                    [
                        new NoteContext(Voice.Soprano, Note.Get(NoteName.C, 4), NoteMotion.Oblique, NoteSpan.None),
                        new NoteContext(Voice.Alto, Note.Get(NoteName.F, 3), NoteMotion.Ascending, NoteSpan.Step),
                        new NoteContext(Voice.Tenor, Note.Get(NoteName.F, 2), NoteMotion.Descending, NoteSpan.Step),
                        new NoteContext(Voice.Bass, Note.Get(NoteName.G, 1), NoteMotion.Ascending, NoteSpan.Leap)
                    ]
                )
            );

            yield return new TestCaseData(
                new ContextualizedChord(
                    new HashSet<ContextualizedNote>
                    {
                        new(
                            Note.Get(NoteName.C, 4),
                            Voice.Soprano,
                            new NoteContext(Voice.Soprano, Note.Get(NoteName.C, 4), NoteMotion.Oblique, NoteSpan.None),
                            new NoteChoice(Voice.Soprano, NoteMotion.Oblique, 0)
                        ),
                        new(
                            Note.Get(NoteName.E, 3),
                            Voice.Alto,
                            new NoteContext(Voice.Alto, Note.Get(NoteName.E, 4), NoteMotion.Oblique, NoteSpan.None),
                            new NoteChoice(Voice.Alto, NoteMotion.Oblique, 0)
                        ),
                        new(
                            Note.Get(NoteName.G, 2),
                            Voice.Tenor,
                            new NoteContext(Voice.Tenor, Note.Get(NoteName.G, 4), NoteMotion.Oblique, NoteSpan.None),
                            new NoteChoice(Voice.Tenor, NoteMotion.Oblique, 0)
                        ),
                        new(
                            Note.Get(NoteName.C, 1),
                            Voice.Bass,
                            new NoteContext(Voice.Bass, Note.Get(NoteName.C, 4), NoteMotion.Oblique, NoteSpan.None),
                            new NoteChoice(Voice.Bass, NoteMotion.Oblique, 0)
                        )
                    },
                    new ChordContext([]),
                    new ChordChoice([])
                ),
                new ContextualizedChord(
                    new HashSet<ContextualizedNote>
                    {
                        new(
                            Note.Get(NoteName.C, 4),
                            Voice.Soprano,
                            new NoteContext(Voice.Soprano, Note.Get(NoteName.C, 4), NoteMotion.Oblique, NoteSpan.None),
                            new NoteChoice(Voice.Soprano, NoteMotion.Oblique, 0)
                        ),
                        new(
                            Note.Get(NoteName.E, 3),
                            Voice.Alto,
                            new NoteContext(Voice.Alto, Note.Get(NoteName.E, 3), NoteMotion.Oblique, NoteSpan.None),
                            new NoteChoice(Voice.Alto, NoteMotion.Oblique, 0)
                        ),
                        new(
                            Note.Get(NoteName.G, 2),
                            Voice.Tenor,
                            new NoteContext(Voice.Tenor, Note.Get(NoteName.G, 2), NoteMotion.Oblique, NoteSpan.None),
                            new NoteChoice(Voice.Tenor, NoteMotion.Oblique, 0)
                        ),
                        new(
                            Note.Get(NoteName.C, 1),
                            Voice.Bass,
                            new NoteContext(Voice.Bass, Note.Get(NoteName.C, 1), NoteMotion.Oblique, NoteSpan.None),
                            new NoteChoice(Voice.Bass, NoteMotion.Oblique, 0)
                        )
                    },
                    new ChordContext([]),
                    new ChordChoice([])
                ),
                new ChordContext(
                    [
                        new NoteContext(Voice.Soprano, Note.Get(NoteName.C, 4), NoteMotion.Oblique, NoteSpan.None),
                        new NoteContext(Voice.Alto, Note.Get(NoteName.E, 3), NoteMotion.Oblique, NoteSpan.None),
                        new NoteContext(Voice.Tenor, Note.Get(NoteName.G, 2), NoteMotion.Oblique, NoteSpan.None),
                        new NoteContext(Voice.Bass, Note.Get(NoteName.C, 1), NoteMotion.Oblique, NoteSpan.None)
                    ]
                )
            );

            yield return new TestCaseData(
                new ContextualizedChord(
                    new HashSet<ContextualizedNote>
                    {
                        new(
                            Note.Get(NoteName.C, 4),
                            Voice.Soprano,
                            new NoteContext(Voice.Soprano, Note.Get(NoteName.C, 4), NoteMotion.Oblique, NoteSpan.None),
                            new NoteChoice(Voice.Soprano, NoteMotion.Oblique, 0)
                        ),
                        new(
                            Note.Get(NoteName.E, 3),
                            Voice.Alto,
                            new NoteContext(Voice.Alto, Note.Get(NoteName.E, 4), NoteMotion.Oblique, NoteSpan.None),
                            new NoteChoice(Voice.Alto, NoteMotion.Oblique, 0)
                        ),
                        new(
                            Note.Get(NoteName.G, 2),
                            Voice.Tenor,
                            new NoteContext(Voice.Tenor, Note.Get(NoteName.G, 4), NoteMotion.Oblique, NoteSpan.None),
                            new NoteChoice(Voice.Tenor, NoteMotion.Oblique, 0)
                        ),
                        new(
                            Note.Get(NoteName.C, 1),
                            Voice.Bass,
                            new NoteContext(Voice.Bass, Note.Get(NoteName.C, 4), NoteMotion.Oblique, NoteSpan.None),
                            new NoteChoice(Voice.Bass, NoteMotion.Oblique, 0)
                        )
                    },
                    new ChordContext([]),
                    new ChordChoice([])
                ),
                new ContextualizedChord(
                    new HashSet<ContextualizedNote>
                    {
                        new(
                            Note.Get(NoteName.C, 5),
                            Voice.Soprano,
                            new NoteContext(Voice.Soprano, Note.Get(NoteName.C, 5), NoteMotion.Oblique, NoteSpan.None),
                            new NoteChoice(Voice.Soprano, NoteMotion.Oblique, 0)
                        ),
                        new(
                            Note.Get(NoteName.E, 2),
                            Voice.Alto,
                            new NoteContext(Voice.Alto, Note.Get(NoteName.E, 2), NoteMotion.Oblique, NoteSpan.None),
                            new NoteChoice(Voice.Alto, NoteMotion.Oblique, 0)
                        ),
                        new(
                            Note.Get(NoteName.G, 4),
                            Voice.Tenor,
                            new NoteContext(Voice.Tenor, Note.Get(NoteName.G, 4), NoteMotion.Oblique, NoteSpan.None),
                            new NoteChoice(Voice.Tenor, NoteMotion.Oblique, 0)
                        ),
                        new(
                            Note.Get(NoteName.C, 0),
                            Voice.Bass,
                            new NoteContext(Voice.Bass, Note.Get(NoteName.C, 0), NoteMotion.Oblique, NoteSpan.None),
                            new NoteChoice(Voice.Bass, NoteMotion.Oblique, 0)
                        )
                    },
                    new ChordContext([]),
                    new ChordChoice([])
                ),
                new ChordContext(
                    [
                        new NoteContext(Voice.Soprano, Note.Get(NoteName.C, 5), NoteMotion.Ascending, NoteSpan.Leap),
                        new NoteContext(Voice.Alto, Note.Get(NoteName.E, 2), NoteMotion.Descending, NoteSpan.Leap),
                        new NoteContext(Voice.Tenor, Note.Get(NoteName.G, 4), NoteMotion.Ascending, NoteSpan.Leap),
                        new NoteContext(Voice.Bass, Note.Get(NoteName.C, 0), NoteMotion.Descending, NoteSpan.Leap)
                    ]
                )
            );
        }
    }
}

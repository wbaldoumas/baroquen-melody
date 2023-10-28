using BaroquenMelody.Library.Composition.Choices;
using BaroquenMelody.Library.Composition.Enums;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Composition.Choices;

[TestFixture]
internal sealed class ChordChoiceTests
{
    [Test]
    public void WhenChordChoicesAreSameReference_TheyAreEqual()
    {
        var note1 = new NoteChoice(Voice.Soprano, NoteMotion.Oblique, 0);
        var note2 = new NoteChoice(Voice.Alto, NoteMotion.Ascending, 2);
        var note3 = new NoteChoice(Voice.Tenor, NoteMotion.Descending, 3);
        var note4 = new NoteChoice(Voice.Bass, NoteMotion.Ascending, 5);

        var chordChoiceA = new ChordChoice(new List<NoteChoice> { note1, note2, note3, note4 });
        var chordChoiceB = chordChoiceA;

        chordChoiceA.Should().BeEquivalentTo(chordChoiceB);
        chordChoiceB.Should().BeEquivalentTo(chordChoiceA);

        chordChoiceA.Equals(chordChoiceB).Should().BeTrue();
        chordChoiceB.Equals(chordChoiceA).Should().BeTrue();

        chordChoiceA.GetHashCode().Should().Be(chordChoiceB.GetHashCode());
        chordChoiceB.GetHashCode().Should().Be(chordChoiceA.GetHashCode());

        (chordChoiceA == chordChoiceB).Should().BeTrue();
        (chordChoiceB == chordChoiceA).Should().BeTrue();

        (chordChoiceA != chordChoiceB).Should().BeFalse();
        (chordChoiceB != chordChoiceA).Should().BeFalse();
    }

    [Test]
    public void WhenChordChoicesHaveSameNoteChoices_TheyAreEqual()
    {
        var note1 = new NoteChoice(Voice.Soprano, NoteMotion.Oblique, 0);
        var note2 = new NoteChoice(Voice.Alto, NoteMotion.Ascending, 2);
        var note3 = new NoteChoice(Voice.Tenor, NoteMotion.Descending, 3);
        var note4 = new NoteChoice(Voice.Bass, NoteMotion.Ascending, 5);

        var chordChoiceA = new ChordChoice(new List<NoteChoice> { note1, note2, note3, note4 });
        var chordChoiceB = new ChordChoice(new List<NoteChoice> { note1, note2, note3, note4 });

        chordChoiceA.Should().BeEquivalentTo(chordChoiceB);
        chordChoiceB.Should().BeEquivalentTo(chordChoiceA);

        chordChoiceA.Equals(chordChoiceB).Should().BeTrue();
        chordChoiceB.Equals(chordChoiceA).Should().BeTrue();

        chordChoiceA.GetHashCode().Should().Be(chordChoiceB.GetHashCode());
        chordChoiceB.GetHashCode().Should().Be(chordChoiceA.GetHashCode());

        (chordChoiceA == chordChoiceB).Should().BeTrue();
        (chordChoiceB == chordChoiceA).Should().BeTrue();

        (chordChoiceA != chordChoiceB).Should().BeFalse();
        (chordChoiceB != chordChoiceA).Should().BeFalse();
    }

    [Test]
    public void WhenOneChordChoiceIsNull_TheyAreNotEqual()
    {
        var note1 = new NoteChoice(Voice.Soprano, NoteMotion.Oblique, 0);
        var note2 = new NoteChoice(Voice.Alto, NoteMotion.Ascending, 2);
        var note3 = new NoteChoice(Voice.Tenor, NoteMotion.Descending, 3);
        var note4 = new NoteChoice(Voice.Bass, NoteMotion.Ascending, 5);

        var chordChoiceA = new ChordChoice(new List<NoteChoice> { note1, note2, note3, note4 });
        ChordChoice? chordChoiceB = null;

        chordChoiceA.Should().NotBeEquivalentTo(chordChoiceB);
        chordChoiceB.Should().NotBeEquivalentTo(chordChoiceA);

        chordChoiceA.Equals(chordChoiceB).Should().BeFalse();
    }

    [Test]
    public void WhenNonDestructiveMutationUsed_InitializerInvoked()
    {
        var note1 = new NoteChoice(Voice.Soprano, NoteMotion.Oblique, 0);
        var note2 = new NoteChoice(Voice.Alto, NoteMotion.Ascending, 2);
        var note3 = new NoteChoice(Voice.Tenor, NoteMotion.Descending, 3);
        var note4 = new NoteChoice(Voice.Bass, NoteMotion.Ascending, 5);

        var chordChoice = new ChordChoice(new List<NoteChoice> { note1, note2, note3, note4 });
        var otherChordChoice = chordChoice with { NoteChoices = new List<NoteChoice> { note3, note4, note1, note2 } };

        otherChordChoice.NoteChoices.Should().HaveElementAt(0, note1);
        otherChordChoice.NoteChoices.Should().HaveElementAt(1, note2);
        otherChordChoice.NoteChoices.Should().HaveElementAt(2, note3);
        otherChordChoice.NoteChoices.Should().HaveElementAt(3, note4);
    }
}

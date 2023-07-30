﻿using BaroquenMelody.Library.Composition.Contexts;
using BaroquenMelody.Library.Composition.Enums;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Composition.Contexts;

[TestFixture]
internal sealed class ChordContextTests
{
    [Test]
    public void WhenChordContextsAreSameReference_TheyAreEqual()
    {
        var context1 = new NoteContext(Voice.Soprano, 60, NoteMotion.Oblique, NoteSpan.None);
        var context2 = new NoteContext(Voice.Alto, 64, NoteMotion.Ascending, NoteSpan.Step);
        var context3 = new NoteContext(Voice.Tenor, 67, NoteMotion.Descending, NoteSpan.Leap);
        var context4 = new NoteContext(Voice.Bass, 72, NoteMotion.Ascending, NoteSpan.Step);

        var chordContextA = new ChordContext(new List<NoteContext> { context1, context2, context3, context4 });
        var chordContextB = chordContextA;

        chordContextA.Should().BeEquivalentTo(chordContextB);
        chordContextB.Should().BeEquivalentTo(chordContextA);

        chordContextA.Equals(chordContextB).Should().BeTrue();
        chordContextB.Equals(chordContextA).Should().BeTrue();

        chordContextA.GetHashCode().Should().Be(chordContextB.GetHashCode());
        chordContextB.GetHashCode().Should().Be(chordContextA.GetHashCode());

        (chordContextA == chordContextB).Should().BeTrue();
        (chordContextB == chordContextA).Should().BeTrue();

        (chordContextA != chordContextB).Should().BeFalse();
        (chordContextB != chordContextA).Should().BeFalse();
    }

    [Test]
    public void WhenChordContextsHaveSameNoteContexts_TheyAreEqual()
    {
        var context1 = new NoteContext(Voice.Soprano, 60, NoteMotion.Oblique, NoteSpan.None);
        var context2 = new NoteContext(Voice.Alto, 64, NoteMotion.Ascending, NoteSpan.Step);
        var context3 = new NoteContext(Voice.Tenor, 67, NoteMotion.Descending, NoteSpan.Leap);
        var context4 = new NoteContext(Voice.Bass, 72, NoteMotion.Ascending, NoteSpan.Step);

        var chordContextA = new ChordContext(new List<NoteContext> { context1, context2, context3, context4 });
        var chordContextB = new ChordContext(new List<NoteContext> { context1, context2, context3, context4 });

        chordContextA.Should().BeEquivalentTo(chordContextB);
        chordContextB.Should().BeEquivalentTo(chordContextA);

        chordContextA.Equals(chordContextB).Should().BeTrue();
        chordContextB.Equals(chordContextA).Should().BeTrue();

        chordContextA.GetHashCode().Should().Be(chordContextB.GetHashCode());
        chordContextB.GetHashCode().Should().Be(chordContextA.GetHashCode());

        (chordContextA == chordContextB).Should().BeTrue();
        (chordContextB == chordContextA).Should().BeTrue();

        (chordContextA != chordContextB).Should().BeFalse();
        (chordContextB != chordContextA).Should().BeFalse();
    }

    [Test]
    public void WhenOneChordContextIsNull_TheyAreNotEqual()
    {
        var context1 = new NoteContext(Voice.Soprano, 60, NoteMotion.Oblique, NoteSpan.None);
        var context2 = new NoteContext(Voice.Alto, 64, NoteMotion.Ascending, NoteSpan.Step);
        var context3 = new NoteContext(Voice.Tenor, 67, NoteMotion.Descending, NoteSpan.Leap);
        var context4 = new NoteContext(Voice.Bass, 72, NoteMotion.Ascending, NoteSpan.Step);

        var chordContextA = new ChordContext(new List<NoteContext> { context1, context2, context3, context4 });
        ChordContext? chordContextB = null;

        chordContextA.Should().NotBeEquivalentTo(chordContextB);
        chordContextB.Should().NotBeEquivalentTo(chordContextA);

        chordContextA.Equals(chordContextB).Should().BeFalse();
    }
}

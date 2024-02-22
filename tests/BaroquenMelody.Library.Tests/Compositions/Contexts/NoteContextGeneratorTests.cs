using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Contexts;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Extensions;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Contexts;

[TestFixture]
internal sealed class NoteContextGeneratorTests
{
    private NoteContextGenerator _noteContextGenerator = null!;

    [SetUp]
    public void Setup() => _noteContextGenerator = new NoteContextGenerator();

    [Test]
    [TestCaseSource(nameof(TestVoiceConfigurations))]
    public void TestGenerateNoteContexts(VoiceConfiguration voiceConfiguration, HashSet<NoteContext> expectedContexts)
    {
        var generatedContexts = _noteContextGenerator.GenerateNoteContexts(voiceConfiguration, Scale.Parse("C Major"));

        generatedContexts.Should().BeEquivalentTo(expectedContexts);
    }

    private static IEnumerable<TestCaseData> TestVoiceConfigurations
    {
        get
        {
            return ((Voice[])Enum.GetValues(typeof(Voice)))
                .Select(voice => new TestCaseData(
                    new VoiceConfiguration(voice, 0.ToNote(), 12.ToNote()),
                    new HashSet<NoteContext>
                    {
                        new(voice, 0.ToNote(), NoteMotion.Descending, NoteSpan.Step),
                        new(voice, 0.ToNote(), NoteMotion.Descending, NoteSpan.Leap),
                        new(voice, 0.ToNote(), NoteMotion.Oblique, NoteSpan.None),
                        new(voice, 2.ToNote(), NoteMotion.Ascending, NoteSpan.Step),
                        new(voice, 2.ToNote(), NoteMotion.Ascending, NoteSpan.Leap),
                        new(voice, 2.ToNote(), NoteMotion.Descending, NoteSpan.Step),
                        new(voice, 2.ToNote(), NoteMotion.Descending, NoteSpan.Leap),
                        new(voice, 2.ToNote(), NoteMotion.Oblique, NoteSpan.None),
                        new(voice, 4.ToNote(), NoteMotion.Ascending, NoteSpan.Step),
                        new(voice, 4.ToNote(), NoteMotion.Ascending, NoteSpan.Leap),
                        new(voice, 4.ToNote(), NoteMotion.Descending, NoteSpan.Step),
                        new(voice, 4.ToNote(), NoteMotion.Descending, NoteSpan.Leap),
                        new(voice, 4.ToNote(), NoteMotion.Oblique, NoteSpan.None),
                        new(voice, 5.ToNote(), NoteMotion.Ascending, NoteSpan.Step),
                        new(voice, 5.ToNote(), NoteMotion.Ascending, NoteSpan.Leap),
                        new(voice, 5.ToNote(), NoteMotion.Descending, NoteSpan.Step),
                        new(voice, 5.ToNote(), NoteMotion.Descending, NoteSpan.Leap),
                        new(voice, 5.ToNote(), NoteMotion.Oblique, NoteSpan.None),
                        new(voice, 7.ToNote(), NoteMotion.Ascending, NoteSpan.Step),
                        new(voice, 7.ToNote(), NoteMotion.Ascending, NoteSpan.Leap),
                        new(voice, 7.ToNote(), NoteMotion.Descending, NoteSpan.Step),
                        new(voice, 7.ToNote(), NoteMotion.Descending, NoteSpan.Leap),
                        new(voice, 7.ToNote(), NoteMotion.Oblique, NoteSpan.None),
                        new(voice, 9.ToNote(), NoteMotion.Ascending, NoteSpan.Step),
                        new(voice, 9.ToNote(), NoteMotion.Ascending, NoteSpan.Leap),
                        new(voice, 9.ToNote(), NoteMotion.Descending, NoteSpan.Step),
                        new(voice, 9.ToNote(), NoteMotion.Descending, NoteSpan.Leap),
                        new(voice, 9.ToNote(), NoteMotion.Oblique, NoteSpan.None),
                        new(voice, 11.ToNote(), NoteMotion.Ascending, NoteSpan.Step),
                        new(voice, 11.ToNote(), NoteMotion.Ascending, NoteSpan.Leap),
                        new(voice, 11.ToNote(), NoteMotion.Descending, NoteSpan.Step),
                        new(voice, 11.ToNote(), NoteMotion.Descending, NoteSpan.Leap),
                        new(voice, 11.ToNote(), NoteMotion.Oblique, NoteSpan.None),
                        new(voice, 12.ToNote(), NoteMotion.Ascending, NoteSpan.Step),
                        new(voice, 12.ToNote(), NoteMotion.Ascending, NoteSpan.Leap),
                        new(voice, 12.ToNote(), NoteMotion.Oblique, NoteSpan.None)
                    }
                ));
        }
    }
}

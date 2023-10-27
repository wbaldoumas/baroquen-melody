using BaroquenMelody.Library.Composition.Configurations;
using BaroquenMelody.Library.Composition.Contexts;
using BaroquenMelody.Library.Composition.Enums;
using FluentAssertions;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Composition.Contexts;

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
        var generatedContexts = _noteContextGenerator.GenerateNoteContexts(voiceConfiguration);

        generatedContexts.Should().BeEquivalentTo(expectedContexts);
    }

    private static IEnumerable<TestCaseData> TestVoiceConfigurations
    {
        get
        {
            return ((Voice[])Enum.GetValues(typeof(Voice))).Select(voice => new TestCaseData(
                new VoiceConfiguration(voice, 1, 5),
                new HashSet<NoteContext>
                {
                    new(voice, 1, NoteMotion.Descending, NoteSpan.Step),
                    new(voice, 1, NoteMotion.Descending, NoteSpan.Leap),
                    new(voice, 1, NoteMotion.Oblique, NoteSpan.None),
                    new(voice, 2, NoteMotion.Ascending, NoteSpan.Step),
                    new(voice, 2, NoteMotion.Ascending, NoteSpan.Leap),
                    new(voice, 2, NoteMotion.Descending, NoteSpan.Step),
                    new(voice, 2, NoteMotion.Descending, NoteSpan.Leap),
                    new(voice, 2, NoteMotion.Oblique, NoteSpan.None),
                    new(voice, 3, NoteMotion.Ascending, NoteSpan.Step),
                    new(voice, 3, NoteMotion.Ascending, NoteSpan.Leap),
                    new(voice, 3, NoteMotion.Descending, NoteSpan.Step),
                    new(voice, 3, NoteMotion.Descending, NoteSpan.Leap),
                    new(voice, 3, NoteMotion.Oblique, NoteSpan.None),
                    new(voice, 4, NoteMotion.Ascending, NoteSpan.Step),
                    new(voice, 4, NoteMotion.Ascending, NoteSpan.Leap),
                    new(voice, 4, NoteMotion.Descending, NoteSpan.Step),
                    new(voice, 4, NoteMotion.Descending, NoteSpan.Leap),
                    new(voice, 4, NoteMotion.Oblique, NoteSpan.None),
                    new(voice, 5, NoteMotion.Ascending, NoteSpan.Step),
                    new(voice, 5, NoteMotion.Ascending, NoteSpan.Leap),
                    new(voice, 5, NoteMotion.Oblique, NoteSpan.None)
                }
            ));
        }
    }
}

using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Domain;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Phrasing;
using BaroquenMelody.Library.Compositions.Rules;
using BaroquenMelody.Library.Infrastructure.Random;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Compositions.Phrasing;

[TestFixture]
internal sealed class CompositionPhraserTests
{
    private ICompositionRule _mockCompositionRule = null!;

    private IThemeSplitter _mockThemeSplitter = null!;

    private IWeightedRandomBooleanGenerator _weightedRandomBooleanGenerator = null!;

    [SetUp]
    public void SetUp()
    {
        _mockCompositionRule = Substitute.For<ICompositionRule>();
        _mockThemeSplitter = Substitute.For<IThemeSplitter>();
        _mockThemeSplitter.SplitThemeIntoPhrases(Arg.Any<BaroquenTheme>()).Returns([]);
        _weightedRandomBooleanGenerator = new WeightedRandomBooleanGenerator();
    }

    [Test]
    public void AttemptPhraseRepetition_ExistingRepeatablePhrase_ShouldRepeat()
    {
        // arrange
        var phraser = CreatePhraser(new PhrasingConfiguration([4], 2, 1, 100));
        var measures = CreateMeasures(8);

        SetRuleEvaluationOutcome(_mockCompositionRule, true);

        // act
        phraser.AttemptPhraseRepetition(measures);

        // assert
        measures.Should().HaveCount(12, because: "a phrase of 4 measures should have been repeated");
    }

    [Test]
    public void AttemptPhraseRepetition_ExistingRepeatableThemePhrase_ShouldRepeat()
    {
        // arrange
        var phraser = CreatePhraser(new PhrasingConfiguration([4], 2, 1, 100));
        var measures = CreateMeasures(8);

        _mockThemeSplitter.SplitThemeIntoPhrases(Arg.Any<BaroquenTheme>()).Returns([
            new RepeatedPhrase { Phrase = measures.Take(4).ToList() }
        ]);

        phraser.AddTheme(new BaroquenTheme
        {
            Recapitulation = measures.Take(4).ToList()
        });

        SetRuleEvaluationOutcome(_mockCompositionRule, true);

        // act
        phraser.AttemptPhraseRepetition(measures);

        // assert
        measures.Should().HaveCount(12, because: "a theme phrase of 4 measures should have been repeated");
    }

    [Test]
    public void AttemptPhraseRepetition_ExistingPhraseNotRepeatable_ShouldNotRepeat()
    {
        // arrange
        var phraser = CreatePhraser(new PhrasingConfiguration([4], 1, 1, 100)); // Max repetitions set to 1
        var measures = CreateMeasures(8);

        SetRuleEvaluationOutcome(_mockCompositionRule, true);

        // act
        // First repetition should succeed
        phraser.AttemptPhraseRepetition(measures);

        // Disallow further new repeated phrase creation
        SetRuleEvaluationOutcome(_mockCompositionRule, false);

        // Trying to repeat should fail since the phrase was already repeated and the rule disallows new phrases
        phraser.AttemptPhraseRepetition(measures);

        // assert
        measures.Should().HaveCount(12, "the phrase should not be repeated more than once");
    }

    [Test]
    public void AttemptPhraseRepetition_NoExistingPhrase_ShouldCreateAndRepeatNewPhrase()
    {
        // arrange
        var phraser = CreatePhraser(new PhrasingConfiguration([2], 2, 0, 100));
        var measures = CreateMeasures(4);

        SetRuleEvaluationOutcome(_mockCompositionRule, true);

        // act
        phraser.AttemptPhraseRepetition(measures);
        phraser.AttemptPhraseRepetition(measures);

        // assert
        measures.Should().HaveCount(8, "a new phrase of 2 measures should be added");
    }

    [Test]
    public void AttemptPhraseRepetition_CannotRepeat_ShouldNotRepeat()
    {
        // arrange
        var phraser = CreatePhraser(new PhrasingConfiguration([4], 2, 2, 100)); // Increase min pool size
        var measures = CreateMeasures(4);

        SetRuleEvaluationOutcome(_mockCompositionRule, false);

        // act
        phraser.AttemptPhraseRepetition(measures);

        // assert
        measures.Should().HaveCount(4, "no phrases should be repeated due to min pool size requirement");
    }

    [Test]
    public void AttemptPhraseRepetition_ProbabilityNotMet_ShouldNotRepeat()
    {
        // arrange
        var phraser = CreatePhraser(new PhrasingConfiguration([4], 2, 1, 0));
        var measures = CreateMeasures(8);

        SetRuleEvaluationOutcome(_mockCompositionRule, true);

        // act
        phraser.AttemptPhraseRepetition(measures);

        // assert
        measures.Should().HaveCount(8, "no phrases should be repeated due to 0% repetition probability");
    }

    [Test]
    public void AttemptPhraseRepetition_WithCoolOffPhrase_ShouldUtilizeCoolOffPhrase()
    {
        // arrange
        var phraser = CreatePhraser(new PhrasingConfiguration([2], 1, 1, 100)); // Ensuring coolOffPhrase can be set
        var measures = CreateMeasures(4);

        SetRuleEvaluationOutcome(_mockCompositionRule, true);

        // act
        phraser.AttemptPhraseRepetition(measures);
        phraser.AttemptPhraseRepetition(measures);

        // assert
        measures.Should().HaveCount(8);
    }

    [Test]
    public void AttemptPhraseRepetition_MeasuresLessThanPhraseLength_ShouldNotCreateOrRepeatPhrase()
    {
        // arrange
        var phraser = CreatePhraser(new PhrasingConfiguration([5], 2, 0, 100)); // Phrase length greater than measures count
        var measures = CreateMeasures(4); // Less than the required phrase length

        SetRuleEvaluationOutcome(_mockCompositionRule, true);

        // act
        phraser.AttemptPhraseRepetition(measures);

        // assert
        measures.Should().HaveCount(4, "no new phrase should be created or repeated due to measures count being less than the phrase length");
    }

    [Test]
    public void AddTheme_invokes_ThemeSplitter()
    {
        // arrange
        var phraser = CreatePhraser(new PhrasingConfiguration([4], 2, 1, 100));

        // act
        phraser.AddTheme(new BaroquenTheme());

        // assert
        _mockThemeSplitter.Received(1).SplitThemeIntoPhrases(Arg.Any<BaroquenTheme>());
    }

    private static List<Measure> CreateMeasures(int count, int beatsPerMeasure = 4)
    {
        var measures = new List<Measure>();

        for (var i = 0; i < count; i++)
        {
            var beats = Enumerable.Range(0, beatsPerMeasure).Select(_ => new Beat(new BaroquenChord([new BaroquenNote(Voice.Soprano, Notes.C4)]))).ToList();
            measures.Add(new Measure(beats, Meter.FourFour));
        }

        return measures;
    }

    private static void SetRuleEvaluationOutcome(ICompositionRule rule, bool outcome) => rule.Evaluate(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>()).Returns(outcome);

    private CompositionPhraser CreatePhraser(PhrasingConfiguration phrasingConfiguration)
    {
        var compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>(),
            phrasingConfiguration,
            BaroquenScale.Parse("C Major"),
            Meter.FourFour,
            16
        );

        return new CompositionPhraser(_mockCompositionRule, _mockThemeSplitter, _weightedRandomBooleanGenerator, compositionConfiguration);
    }
}

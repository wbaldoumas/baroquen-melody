using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.Motifs;
using BaroquenMelody.Library.Ornamentation;
using BaroquenMelody.Library.Phrasing;
using BaroquenMelody.Library.Rules;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.MusicTheory;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Tests.Phrasing;

[TestFixture]
internal sealed class CompositionPhraserTests
{
    private ICompositionRule _mockCompositionRule = null!;

    private IThemeSplitter _mockThemeSplitter = null!;

    private IWeightedRandomBooleanGenerator _weightedRandomBooleanGenerator = null!;

    private ILogger _mockLogger = null!;

    private IMotifBankFactory _mockMotifBankFactory = null!;

    private IMotifDeveloper _mockMotifDeveloper = null!;

    private ICadentialTrillApplicator _mockCadentialTrillApplicator = null!;

    [SetUp]
    public void SetUp()
    {
        _mockCadentialTrillApplicator = Substitute.For<ICadentialTrillApplicator>();
        _mockCompositionRule = Substitute.For<ICompositionRule>();
        _mockThemeSplitter = Substitute.For<IThemeSplitter>();
        _mockLogger = Substitute.For<ILogger>();
        _mockThemeSplitter.SplitThemeIntoPhrases(Arg.Any<BaroquenTheme>()).Returns([]);
        _weightedRandomBooleanGenerator = new WeightedRandomBooleanGenerator();

        // by default, motivic development is a no-op so these tests pin the verbatim repetition behavior.
        _mockMotifBankFactory = Substitute.For<IMotifBankFactory>();
        _mockMotifBankFactory.Create(Arg.Any<BaroquenTheme>()).Returns(new MotifBank(new Dictionary<Instrument, AnchoredMotif>()));
        _mockMotifDeveloper = Substitute.For<IMotifDeveloper>();
    }

    [Test]
    public void AttemptPhraseRepetition_ExistingRepeatablePhrase_ShouldRepeat()
    {
        // arrange
        var phraser = CreatePhraser(new PhrasingConfiguration([4], 2, 1));
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
        var phraser = CreatePhraser(new PhrasingConfiguration([4], 2, 1));
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
        var phraser = CreatePhraser(new PhrasingConfiguration([4], 1, 1)); // Max repetitions set to 1
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
        var phraser = CreatePhraser(new PhrasingConfiguration([2], 2, 0));
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
        var phraser = CreatePhraser(new PhrasingConfiguration([4], 2)); // Increase min pool size
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
        var phraser = CreatePhraser(new PhrasingConfiguration([2], 2, 1)); // Ensuring coolOffPhrase can be set
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
        var phraser = CreatePhraser(new PhrasingConfiguration([5], 2, 0)); // Phrase length greater than measures count
        var measures = CreateMeasures(4); // Less than the required phrase length

        SetRuleEvaluationOutcome(_mockCompositionRule, true);

        // act
        phraser.AttemptPhraseRepetition(measures);

        // assert
        measures.Should().HaveCount(4, "no new phrase should be created or repeated due to measures count being less than the phrase length");
    }

    [Test]
    public void AttemptPhraseRepetition_WhenAnExistingPhraseIsRepeated_AppliesTheCadentialTrillToTheLiveSeam()
    {
        // arrange
        var phraser = CreatePhraser(new PhrasingConfiguration([4], 2, 1));
        var measures = CreateMeasures(8);

        _mockThemeSplitter.SplitThemeIntoPhrases(Arg.Any<BaroquenTheme>()).Returns([
            new RepeatedPhrase { Phrase = measures.Take(4).ToList() }
        ]);

        phraser.AddTheme(new BaroquenTheme { Recapitulation = measures.Take(4).ToList() });

        SetRuleEvaluationOutcome(_mockCompositionRule, true);

        var seamMeasure = measures[^1];
        var expectedPenultimateChord = seamMeasure.Beats[^2].Chord;
        var expectedFinalChord = seamMeasure.Beats[^1].Chord;

        // act
        phraser.AttemptPhraseRepetition(measures);

        // assert
        measures.Should().HaveCount(12, because: "the phrase repetition whose seam is being trilled should have happened");

        _mockCadentialTrillApplicator
            .Received(1)
            .ApplyTrill(
                Arg.Is<BaroquenChord>(chord => ReferenceEquals(chord, expectedPenultimateChord)),
                Arg.Is<BaroquenChord>(chord => ReferenceEquals(chord, expectedFinalChord)));
    }

    [Test]
    public void AttemptPhraseRepetition_WhenANewPhraseIsCreatedAndRepeated_AppliesTheCadentialTrillToTheLiveSeam()
    {
        // arrange
        var phraser = CreatePhraser(new PhrasingConfiguration([2], 2, 0));
        var measures = CreateMeasures(4);

        SetRuleEvaluationOutcome(_mockCompositionRule, true);

        var seamMeasure = measures[^1];
        var expectedPenultimateChord = seamMeasure.Beats[^2].Chord;
        var expectedFinalChord = seamMeasure.Beats[^1].Chord;

        // act
        phraser.AttemptPhraseRepetition(measures);

        // assert
        measures.Should().HaveCount(6, because: "the phrase repetition whose seam is being trilled should have happened");

        _mockCadentialTrillApplicator
            .Received(1)
            .ApplyTrill(
                Arg.Is<BaroquenChord>(chord => ReferenceEquals(chord, expectedPenultimateChord)),
                Arg.Is<BaroquenChord>(chord => ReferenceEquals(chord, expectedFinalChord)));
    }

    [Test]
    public void AttemptPhraseRepetition_WhenTheSeamMeasureHasASingleBeat_DoesNotApplyTheCadentialTrill()
    {
        // arrange
        var phraser = CreatePhraser(new PhrasingConfiguration([2], 2, 0));
        var measures = CreateMeasures(4, beatsPerMeasure: 1);

        SetRuleEvaluationOutcome(_mockCompositionRule, true);

        // act
        phraser.AttemptPhraseRepetition(measures);

        // assert
        measures.Should().HaveCount(6, because: "the repetition should still happen; only the trill needs two seam beats");

        _mockCadentialTrillApplicator.DidNotReceive().ApplyTrill(Arg.Any<BaroquenChord>(), Arg.Any<BaroquenChord>());
    }

    [Test]
    public void AddTheme_invokes_ThemeSplitter()
    {
        // arrange
        var phraser = CreatePhraser(new PhrasingConfiguration([4], 2, 1));

        // act
        phraser.AddTheme(new BaroquenTheme());

        // assert
        _mockThemeSplitter.Received(1).SplitThemeIntoPhrases(Arg.Any<BaroquenTheme>());
    }

    [Test]
    public void AttemptPhraseRepetition_WhenDevelopedRestatementPassesTheRuleGate_AppendsTheDevelopedRestatement()
    {
        // arrange
        var phraser = CreatePhraser(new PhrasingConfiguration([4], 2, 1));
        var measures = CreateMeasures(8);
        var themePhrase = measures.Take(4).ToList();

        _mockThemeSplitter.SplitThemeIntoPhrases(Arg.Any<BaroquenTheme>()).Returns([new RepeatedPhrase { Phrase = themePhrase }]);
        phraser.AddTheme(new BaroquenTheme { Recapitulation = themePhrase });

        // the developer offers a restatement voiced on a distinct note so it can be told apart from the verbatim phrase.
        _mockMotifDeveloper.TryDevelop(Arg.Any<MotifBank>(), Arg.Any<IReadOnlyList<Measure>>()).Returns(CreateMeasures(4, note: Notes.E4));

        SetRuleEvaluationOutcome(_mockCompositionRule, true);

        // act
        phraser.AttemptPhraseRepetition(measures);

        // assert
        measures.Should().HaveCount(12);
        AppendedNotes(measures, fromIndex: 8).Should().OnlyContain(raw => raw == Notes.E4, "the developed restatement passed the rule gate");
    }

    [Test]
    public void AttemptPhraseRepetition_WhenDevelopedRestatementFailsTheRuleGate_FallsBackToVerbatim()
    {
        // arrange
        var phraser = CreatePhraser(new PhrasingConfiguration([4], 2, 1));
        var measures = CreateMeasures(8);
        var themePhrase = measures.Take(4).ToList();

        _mockThemeSplitter.SplitThemeIntoPhrases(Arg.Any<BaroquenTheme>()).Returns([new RepeatedPhrase { Phrase = themePhrase }]);
        phraser.AddTheme(new BaroquenTheme { Recapitulation = themePhrase });

        _mockMotifDeveloper.TryDevelop(Arg.Any<MotifBank>(), Arg.Any<IReadOnlyList<Measure>>()).Returns(CreateMeasures(4, note: Notes.E4));

        // the verbatim seam (C4) passes, but any developed chord (E4) is rejected, forcing the verbatim fallback.
        _mockCompositionRule.Evaluate(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>()).Returns(true);
        _mockCompositionRule.Evaluate(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Is<BaroquenChord>(chord => chord.Notes.Any(note => note.Raw == Notes.E4))).Returns(false);

        // act
        phraser.AttemptPhraseRepetition(measures);

        // assert
        measures.Should().HaveCount(12);
        AppendedNotes(measures, fromIndex: 8).Should().OnlyContain(raw => raw == Notes.C4, "the developed restatement was rejected, so the verbatim phrase is appended");
    }

    [Test]
    public void AttemptPhraseRepetition_WhenADevelopedInteriorChordFailsTheRuleGate_FallsBackToVerbatim()
    {
        // arrange
        var phraser = CreatePhraser(new PhrasingConfiguration([4], 2, 1));
        var measures = CreateMeasures(8);
        var themePhrase = measures.Take(4).ToList();

        _mockThemeSplitter.SplitThemeIntoPhrases(Arg.Any<BaroquenTheme>()).Returns([new RepeatedPhrase { Phrase = themePhrase }]);
        phraser.AddTheme(new BaroquenTheme { Recapitulation = themePhrase });

        // the developed candidate's FIRST chord is acceptable (C4) but a later interior chord is not (E4); a gate that
        // validated only the seam/first chord would wrongly accept it, so this discriminates the whole-phrase walk.
        var developedCandidate = CreateMeasures(1, note: Notes.C4).Concat(CreateMeasures(3, note: Notes.E4)).ToList();
        _mockMotifDeveloper.TryDevelop(Arg.Any<MotifBank>(), Arg.Any<IReadOnlyList<Measure>>()).Returns(developedCandidate);

        _mockCompositionRule.Evaluate(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>()).Returns(true);
        _mockCompositionRule.Evaluate(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Is<BaroquenChord>(chord => chord.Notes.Any(note => note.Raw == Notes.E4))).Returns(false);

        // act
        phraser.AttemptPhraseRepetition(measures);

        // assert: the full-phrase walk rejects the candidate on its interior E4 chord, so the verbatim phrase is appended.
        measures.Should().HaveCount(12);
        AppendedNotes(measures, fromIndex: 8).Should().NotContain(raw => raw == Notes.E4, "the developed candidate failed the full-phrase rule walk");
        AppendedNotes(measures, fromIndex: 8).Should().OnlyContain(raw => raw == Notes.C4);
    }

    private static List<Measure> CreateMeasures(int count, int beatsPerMeasure = 4, Note? note = null)
    {
        var raw = note ?? Notes.C4;
        var measures = new List<Measure>();

        for (var i = 0; i < count; i++)
        {
            var beats = Enumerable.Range(0, beatsPerMeasure).Select(_ => new Beat(new BaroquenChord([new BaroquenNote(Instrument.One, raw, MusicalTimeSpan.Half)]))).ToList();
            measures.Add(new Measure(beats, Meter.FourFour));
        }

        return measures;
    }

    private static IEnumerable<Note> AppendedNotes(List<Measure> measures, int fromIndex) =>
        measures.Skip(fromIndex).SelectMany(static measure => measure.Beats).Select(static beat => beat.Chord[Instrument.One].Raw);

    private static void SetRuleEvaluationOutcome(ICompositionRule rule, bool outcome) => rule.Evaluate(Arg.Any<IReadOnlyList<BaroquenChord>>(), Arg.Any<BaroquenChord>()).Returns(outcome);

    private CompositionPhraser CreatePhraser(PhrasingConfiguration phrasingConfiguration)
    {
        var compositionConfiguration = TestCompositionConfigurations.Get(2) with { PhrasingConfiguration = phrasingConfiguration };

        return new CompositionPhraser(_mockCompositionRule, _mockThemeSplitter, _weightedRandomBooleanGenerator, new ThreadLocalRandomProvider(), _mockLogger, compositionConfiguration, _mockMotifBankFactory, _mockMotifDeveloper, _mockCadentialTrillApplicator);
    }
}

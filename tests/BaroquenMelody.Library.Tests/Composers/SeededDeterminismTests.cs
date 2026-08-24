using Atrea.Utilities.Enums;
using BaroquenMelody.Library.Choices;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Enums;
using BaroquenMelody.Library.MusicTheory.Enums;
using BaroquenMelody.Library.Rules.Enums;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using System.Globalization;
using System.Text;

namespace BaroquenMelody.Library.Tests.Composers;

/// <summary>
///     Audit: determinism-1. A seeded composition must depend on its seed and configuration only. Today the
///     candidate and rule sets are <c>FrozenSet</c>s of records, and a frozen set enumerates in hash-bucket order;
///     a record's hash folds in the identity hash of its <c>EqualityContract</c> type, which the runtime assigns on
///     first request from process history. The walk therefore changes with whatever ran earlier in the process.
/// </summary>
[TestFixture]
internal sealed class SeededDeterminismTests
{
    [Test]
    public void NoteChoiceGenerator_EnumeratesCandidatesInGenerationOrder()
    {
        var expected = Enumerable
            .Range(1, CompositionConfiguration.MaxScaleStepChange)
            .SelectMany(static scaleStepChange => new[] { NoteMotion.Ascending, NoteMotion.Descending }.Select(noteMotion => new NoteChoice(Instrument.One, noteMotion, (byte)scaleStepChange)))
            .Append(new NoteChoice(Instrument.One, NoteMotion.Oblique, 0))
            .ToList();

        var actual = new NoteChoiceGenerator().GenerateNoteChoices(Instrument.One).ToList();

        actual.Should().Equal(expected, "the candidate order feeds every seeded draw, so it must not depend on hash-bucket order");
    }

    [Test]
    public void DefaultCompositionRules_EnumerateInDeclarationOrder()
    {
        var expected = EnumUtils<CompositionRule>.AsEnumerable().ToList();

        var actual = AggregateCompositionRuleConfiguration.Default.Configurations.Select(static configuration => configuration.Rule).ToList();

        actual.Should().Equal(expected, "rule order decides evaluation and draw order, so it must not depend on hash-bucket order");
    }

    [Test]
    public void Compose_SameSeedTwiceInOneProcess_ProducesTheSameNotes()
    {
        var configuration = TestCompositionConfigurations.Get(3, 8, NoteName.A, Mode.Aeolian) with
        {
            ShuffleOrnamentationProcessors = false,
        };

        foreach (var seed in Enumerable.Range(1, 5))
        {
            var first = Fingerprint(ComposerGraph.Create(configuration, seed).Composer.Compose(CancellationToken.None));
            var second = Fingerprint(ComposerGraph.Create(configuration, seed).Composer.Compose(CancellationToken.None));

            second.Should().Be(first, $"seed {seed} composed twice in one process should be identical");
        }
    }

    [Test]
    public void Compose_SameSeedWithABusyBackgroundThread_ProducesTheSameNotes()
    {
        var configuration = TestCompositionConfigurations.Get(3, 8, NoteName.A, Mode.Aeolian) with
        {
            ShuffleOrnamentationProcessors = false,
        };

        var quiet = Fingerprint(ComposerGraph.Create(configuration, 3).Composer.Compose(CancellationToken.None));

        using var stop = new CancellationTokenSource();
        var spinner = Task.Run(
            () =>
            {
                var sink = new List<object>();

                while (!stop.IsCancellationRequested)
                {
                    sink.Add(new HashSet<int>(Enumerable.Range(0, 8)));

                    if (sink.Count > 1024)
                    {
                        sink.Clear();
                    }
                }
            },
            CancellationToken.None);

        var busy = Fingerprint(ComposerGraph.Create(configuration, 3).Composer.Compose(CancellationToken.None));

        stop.Cancel();
        spinner.Wait(TimeSpan.FromSeconds(5));

        busy.Should().Be(quiet, "a seeded composition must not depend on other threads");
    }

    /// <summary>
    ///     Serializes every principal and ornament note so two compositions can be compared exactly. The
    ///     cross-process experiment behind determinism-1 wrote this fingerprint to a file from one process that
    ///     composed seed 3 directly and from another that first ran a mocked <c>ComposerTests</c> case (or simply
    ///     requested a thousand identity hash codes); the two pieces differed from the second note of the subject.
    /// </summary>
    private static string Fingerprint(Composition composition)
    {
        var builder = new StringBuilder();

        foreach (var measure in composition.Measures)
        {
            foreach (var beat in measure.Beats)
            {
                foreach (var note in beat.Chord.Notes)
                {
                    AppendNote(builder, note);
                }

                builder.Append('|');
            }

            builder.Append('\n');
        }

        return builder.ToString();
    }

    private static void AppendNote(StringBuilder builder, BaroquenNote note)
    {
        builder.Append(CultureInfo.InvariantCulture, $"{note.Instrument}:{note.Raw}:{note.MusicalTimeSpan}:{note.OrnamentationType}:{note.Velocity}");

        foreach (var ornamentation in note.Ornamentations)
        {
            builder.Append('(');
            AppendNote(builder, ornamentation);
            builder.Append(')');
        }

        builder.Append(';');
    }
}

using BaroquenMelody.Library.Compositions.Choices;
using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Compositions.Enums;
using BaroquenMelody.Library.Compositions.Evaluations.Rules;
using BaroquenMelody.Library.Compositions.Extensions;
using BaroquenMelody.Library.Compositions.Strategies;
using FluentAssertions;
using Melanchall.DryWetMidi.MusicTheory;
using NSubstitute;
using NUnit.Framework;
using System.Numerics;

namespace BaroquenMelody.Library.Tests.Compositions.Strategies;

[TestFixture]
internal sealed class CompositionStrategyTests
{
    private const byte MinSopranoPitch = 60;

    private const byte MaxSopranoPitch = 72;

    private const byte MinAltoPitch = 48;

    private const byte MaxAltoPitch = 60;

    private const byte MinTenorPitch = 36;

    private const byte MaxTenorPitch = 48;

    private const byte MinBassPitch = 24;

    private const byte MaxBassPitch = 36;

    private static readonly BigInteger MockChordChoiceCount = 5;

    private IChordChoiceRepository _mockChordChoiceRepository = default!;

    private ICompositionRule _mockCompositionRule = default!;

    private CompositionStrategy _compositionStrategy = default!;

    private CompositionConfiguration _compositionConfiguration = default!;

    [SetUp]
    public void Setup()
    {
        _mockChordChoiceRepository = Substitute.For<IChordChoiceRepository>();
        _mockCompositionRule = Substitute.For<ICompositionRule>();

        _mockChordChoiceRepository.Count.Returns(MockChordChoiceCount);

        _compositionConfiguration = new CompositionConfiguration(
            new HashSet<VoiceConfiguration>
            {
                new(Voice.Soprano, MinSopranoPitch.ToNote(), MaxSopranoPitch.ToNote()),
                new(Voice.Alto, MinAltoPitch.ToNote(), MaxAltoPitch.ToNote()),
                new(Voice.Tenor, MinTenorPitch.ToNote(), MaxTenorPitch.ToNote()),
                new(Voice.Bass, MinBassPitch.ToNote(), MaxBassPitch.ToNote())
            },
            Scale.Parse("C Major"),
            Meter.FourFour,
            CompositionLength: 100
        );

        _compositionStrategy = new CompositionStrategy(
            _mockChordChoiceRepository,
            _mockCompositionRule,
            _compositionConfiguration
        );
    }

    [Test]
    public void GetInitialChord_generates_valid_chord()
    {
        // run this test a bunch to account for randomness
        for (var i = 0; i < 1000; ++i)
        {
            // act
            var chord = _compositionStrategy.GenerateInitialChord();

            // assert
            chord.Notes.Count().Should().Be(4);

            // since there are four voices but only three unique notes in a C Major chord, there should be at least one voice with a repeated note
            chord.Notes.GroupBy(note => note.Raw.NoteName).Should().HaveCount(3).And.OnlyContain(grouping => grouping.Count() <= 2);

            foreach (var contextualizedNote in chord.Notes)
            {
                contextualizedNote.Raw.NoteName.Should().BeOneOf(
                    NoteName.C,
                    NoteName.E,
                    NoteName.G
                );
            }
        }
    }
}

using BaroquenMelody.Library.Motifs;
using FluentAssertions;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace BaroquenMelody.Library.Tests.Motifs;

[TestFixture]
internal sealed class MotifTransformationsTests
{
    [Test]
    public void Invert_NegatesEveryDelta_KeepsDurations_AndHeadZero()
    {
        // act
        var inverted = MotifTransformations.Invert(BuildSampleMotif());

        // assert
        inverted.Gestures.Should().Equal(
            new MotivicGesture(0, MusicalTimeSpan.Quarter),
            new MotivicGesture(-2, MusicalTimeSpan.Eighth),
            new MotivicGesture(1, MusicalTimeSpan.Half)
        );

        inverted.Gestures[0].ScaleStepDelta.Should().Be(0);
    }

    [Test]
    public void Invert_IsInvolution()
    {
        // arrange
        var motif = BuildSampleMotif();

        // act
        var doublyInverted = MotifTransformations.Invert(MotifTransformations.Invert(motif));

        // assert
        doublyInverted.Gestures.Should().Equal(motif.Gestures);
    }

    [Test]
    public void Retrograde_IsRebuild_NotNaiveListReverse()
    {
        // act
        var retrograde = MotifTransformations.Retrograde(BuildSampleMotif());

        // assert: durations reverse (Half, Eighth, Quarter) and deltas are rebuilt (0, +1, -2). A naive list reverse
        // would instead yield [(-1, Half), (+2, Eighth), (0, Quarter)], which a non-zero head and mispaired deltas catch.
        retrograde.Gestures.Should().Equal(
            new MotivicGesture(0, MusicalTimeSpan.Half),
            new MotivicGesture(1, MusicalTimeSpan.Eighth),
            new MotivicGesture(-2, MusicalTimeSpan.Quarter)
        );
    }

    [Test]
    public void Retrograde_IsInvolution_AndPreservesHeadZero()
    {
        // arrange
        var motif = BuildSampleMotif();

        // act
        var retrograde = MotifTransformations.Retrograde(motif);
        var doubleRetrograde = MotifTransformations.Retrograde(retrograde);

        // assert
        retrograde.Gestures[0].ScaleStepDelta.Should().Be(0);
        doubleRetrograde.Gestures.Should().Equal(motif.Gestures);
    }

    [Test]
    public void Augment_ScalesDurationsOnly_DeltasUnchanged()
    {
        // act
        var augmented = MotifTransformations.Augment(BuildSampleMotif(), 2);

        // assert: each duration doubles exactly; deltas are untouched.
        augmented.Gestures.Should().Equal(
            new MotivicGesture(0, MusicalTimeSpan.Half),
            new MotivicGesture(2, MusicalTimeSpan.Quarter),
            new MotivicGesture(-1, MusicalTimeSpan.Whole)
        );
    }

    [Test]
    public void Diminish_ShrinksDurationsOnly_DeltasUnchanged()
    {
        // act
        var diminished = MotifTransformations.Diminish(BuildSampleMotif(), 2);

        // assert: each duration halves exactly; deltas are untouched; all results stay at or above the 1/32 floor.
        diminished.Gestures.Should().Equal(
            new MotivicGesture(0, MusicalTimeSpan.Eighth),
            new MotivicGesture(2, MusicalTimeSpan.Sixteenth),
            new MotivicGesture(-1, MusicalTimeSpan.Quarter)
        );
    }

    [Test]
    public void DiminishAfterAugment_IsExactIdentity_AndViceVersa()
    {
        // arrange
        var motif = BuildSampleMotif();

        // act
        var augmentThenDiminish = MotifTransformations.Diminish(MotifTransformations.Augment(motif, 4), 4);
        var diminishThenAugment = MotifTransformations.Augment(MotifTransformations.Diminish(motif, 2), 2);

        // assert: exact rational scaling round-trips in both orders (no double rounding).
        augmentThenDiminish.Gestures.Should().Equal(motif.Gestures);
        diminishThenAugment.Gestures.Should().Equal(motif.Gestures);
    }

    [Test]
    public void Diminish_AtFloorBoundary_Succeeds_BelowFloor_Throws()
    {
        // arrange: a single sixteenth note; halving lands exactly on the 1/32 floor (inclusive).
        var motif = new Motif([new MotivicGesture(0, MusicalTimeSpan.Sixteenth)]);

        // act
        var atBoundary = MotifTransformations.Diminish(motif, 2);
        var belowFloorByThree = () => MotifTransformations.Diminish(motif, 3);
        var belowFloorByFour = () => MotifTransformations.Diminish(motif, 4);

        // assert
        atBoundary.Gestures[0].Duration.Should().Be(MusicalTimeSpan.ThirtySecond);
        belowFloorByThree.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("factor");
        belowFloorByFour.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("factor");
    }

    [Test]
    public void AugmentAndDiminish_RejectNonPositiveFactor_AcceptOne()
    {
        // arrange
        var motif = BuildSampleMotif();

        // act
        var augmentByZero = () => MotifTransformations.Augment(motif, 0);
        var augmentByNegative = () => MotifTransformations.Augment(motif, -1);
        var diminishByZero = () => MotifTransformations.Diminish(motif, 0);
        var diminishByNegative = () => MotifTransformations.Diminish(motif, -2);

        // assert: non-positive factors are rejected; a factor of one is identity-on-time.
        augmentByZero.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("factor");
        augmentByNegative.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("factor");
        diminishByZero.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("factor");
        diminishByNegative.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("factor");

        MotifTransformations.Augment(motif, 1).Gestures.Should().Equal(motif.Gestures);
        MotifTransformations.Diminish(motif, 1).Gestures.Should().Equal(motif.Gestures);
    }

    [Test]
    public void Fragment_SlicesRange_RezerosHead_KeepsInnerDeltasAndDurations()
    {
        // act: slice [1, 3) of [(0,Quarter),(+2,Eighth),(-1,Half)].
        var fragment = MotifTransformations.Fragment(BuildSampleMotif(), 1, 2);

        // assert: the new head drops its inbound +2 (re-zeroed); the remaining delta and both durations are verbatim.
        fragment.Gestures.Should().Equal(
            new MotivicGesture(0, MusicalTimeSpan.Eighth),
            new MotivicGesture(-1, MusicalTimeSpan.Half)
        );

        fragment.Gestures[0].ScaleStepDelta.Should().Be(0);
    }

    [Test]
    public void Fragment_FullRange_EqualsIdentity()
    {
        // arrange
        var motif = BuildSampleMotif();

        // act
        var fragment = MotifTransformations.Fragment(motif, 0, motif.Gestures.Count);

        // assert: re-zeroing an already-zero head over the whole range reproduces the original.
        fragment.Gestures.Should().Equal(motif.Gestures);
    }

    [Test]
    public void Fragment_OutOfRange_Throws()
    {
        // arrange
        var motif = BuildSampleMotif();
        var emptyMotif = new Motif([]);

        // act
        var negativeStart = () => MotifTransformations.Fragment(motif, -1, 1);
        var zeroLength = () => MotifTransformations.Fragment(motif, 0, 0);
        var overrun = () => MotifTransformations.Fragment(motif, 2, 2);
        var startAtCount = () => MotifTransformations.Fragment(motif, 3, 1);
        var emptySource = () => MotifTransformations.Fragment(emptyMotif, 0, 1);

        // assert: each guard is pinned by parameter name so collapsing two of them cannot pass unnoticed.
        negativeStart.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("start");
        zeroLength.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("length");
        overrun.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("start + length");
        startAtCount.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("start + length");
        emptySource.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("start + length");
    }

    [Test]
    public void AllTransforms_DoNotMutateInput_AndPreserveHeadZero()
    {
        // arrange
        var motif = BuildSampleMotif();
        var snapshot = motif.Gestures.ToList();

        // act
        var results = new[]
        {
            MotifTransformations.Invert(motif),
            MotifTransformations.Retrograde(motif),
            MotifTransformations.Augment(motif, 2),
            MotifTransformations.Diminish(motif, 2),
            MotifTransformations.Fragment(motif, 1, 2)
        };

        // assert: the input is never mutated, every result is a fresh allocation, and the head-zero invariant holds.
        motif.Gestures.Should().Equal(snapshot);
        results.Should().OnlyContain(result => !ReferenceEquals(result.Gestures, motif.Gestures));
        results.Should().OnlyContain(result => result.Gestures[0].ScaleStepDelta == 0);
    }

    [Test]
    public void StructuralTransforms_HandleEmptyMotif()
    {
        // arrange
        var emptyMotif = new Motif([]);

        // act
        var invertEmpty = () => MotifTransformations.Invert(emptyMotif);
        var augmentEmptyByZero = () => MotifTransformations.Augment(emptyMotif, 0);

        // assert: the structural and duration transforms are total on the empty motif; factor is still validated first.
        MotifTransformations.Invert(emptyMotif).Gestures.Should().BeEmpty();
        MotifTransformations.Retrograde(emptyMotif).Gestures.Should().BeEmpty();
        MotifTransformations.Augment(emptyMotif, 2).Gestures.Should().BeEmpty();
        MotifTransformations.Diminish(emptyMotif, 2).Gestures.Should().BeEmpty();
        invertEmpty.Should().NotThrow();
        augmentEmptyByZero.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("factor");
    }

    [Test]
    public void Identity_ReturnsTheSameInstance()
    {
        // arrange
        var motif = BuildSampleMotif();

        // act
        var identity = MotifTransformations.Identity(motif);

        // assert: the input is immutable, so identity aliases it rather than copying.
        ReferenceEquals(identity, motif).Should().BeTrue();
    }

    [Test]
    public void SingleNoteMotif_InvertAndRetrograde_EqualIdentity()
    {
        // arrange: at length one, both transforms collapse to identity (-0 == 0; the rebuilt head delta is 0).
        var motif = new Motif([new MotivicGesture(0, MusicalTimeSpan.Quarter)]);

        // act
        var inverted = MotifTransformations.Invert(motif);
        var retrograde = MotifTransformations.Retrograde(motif);

        // assert
        inverted.Gestures.Should().Equal(new MotivicGesture(0, MusicalTimeSpan.Quarter));
        retrograde.Gestures.Should().Equal(new MotivicGesture(0, MusicalTimeSpan.Quarter));
    }

    [Test]
    public void Diminish_AppliesTheFloorPerGesture_NotJustToTheHead()
    {
        // arrange: the head (Half) stays above the floor under both factors; the later Sixteenth underflows only at /3
        // (1/16 / 3 = 1/48 < 1/32), so a head-only floor check would wrongly accept it.
        var motif = new Motif(
        [
            new MotivicGesture(0, MusicalTimeSpan.Half),
            new MotivicGesture(2, MusicalTimeSpan.Sixteenth)
        ]);

        // act
        var underflowsAtThree = () => MotifTransformations.Diminish(motif, 3);

        // assert: a non-head underflow throws, and /2 (the Sixteenth lands exactly on the 1/32 floor) succeeds.
        underflowsAtThree.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("factor");

        MotifTransformations.Diminish(motif, 2).Gestures.Should().Equal(
            new MotivicGesture(0, MusicalTimeSpan.Quarter),
            new MotivicGesture(2, MusicalTimeSpan.ThirtySecond)
        );
    }

    [Test]
    public void DiminishThenAugment_IsExactThroughNonDyadicDurations()
    {
        // arrange: a quarter diminished by 3 is 1/12 - a non-power-of-two fraction above the 1/32 floor. Augmenting by 3
        // must return exactly to a quarter, which only holds with exact rational arithmetic (not the lossy double path).
        var motif = new Motif([new MotivicGesture(0, MusicalTimeSpan.Quarter)]);

        // act
        var diminished = MotifTransformations.Diminish(motif, 3);
        var roundTripped = MotifTransformations.Augment(diminished, 3);

        // assert
        diminished.Gestures[0].Duration.Should().Be(new MusicalTimeSpan(1, 12));
        roundTripped.Gestures.Should().Equal(motif.Gestures);
    }

    // The shared sample motif: head + an ascending third + a descending step, with three distinct durations so that
    // rebuild-vs-reverse and delta-vs-duration bugs are observable. Absolute scale indices fold to [0, +2, +1].
    private static Motif BuildSampleMotif() => new(
    [
        new MotivicGesture(0, MusicalTimeSpan.Quarter),
        new MotivicGesture(2, MusicalTimeSpan.Eighth),
        new MotivicGesture(-1, MusicalTimeSpan.Half)
    ]);
}

using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;
using BaroquenMelody.Library.Domain;
using BaroquenMelody.Library.Extensions;
using BaroquenMelody.Library.MusicTheory.Enums;
using Melanchall.DryWetMidi.MusicTheory;
using Note = Melanchall.DryWetMidi.MusicTheory.Note;

namespace BaroquenMelody.Library.Forms;

/// <inheritdoc cref="IGroundBassPlanner"/>
/// <remarks>
///     Feasibility lives in <see cref="GroundBassFeasibilityAnalyzer"/> (the same scan the UI runs to warn
///     about shrinking repertoires); the planner draws once among the feasible patterns in bank order, so
///     plan creation costs exactly one draw whenever any ground fits. A configured pattern narrows the draw
///     to itself when it fits (still one draw, keeping the stream aligned across selections); a configured
///     pattern the range cannot host yields no plan, falling back to the fugue the way an empty bank does.
///     The tonal plan is derived without any further draw: the foreign block's size and position are pure
///     functions of the statement count, and the relative key either qualifies (range-feasible rendering,
///     singable seams) or the plan simply stays home-only.
/// </remarks>
internal sealed class GroundBassPlanner(
    CompositionConfiguration compositionConfiguration,
    IRandomProvider randomProvider
) : IGroundBassPlanner
{
    /// <summary>
    ///     Every composition states the ground at least twice: the opening solo statement and one full-texture
    ///     statement, whatever the configured minimum measure count.
    /// </summary>
    private const int MinStatementCount = 2;

    /// <summary>
    ///     Modulation needs the solo announcement plus one accompanied statement to establish home, at least
    ///     one foreign statement, and a home return before the close - four statements at minimum.
    /// </summary>
    private const int MinStatementCountForModulation = 4;

    /// <summary>
    ///     The journey departs only after the solo announcement and one accompanied statement have
    ///     established the home key.
    /// </summary>
    private const int MinHomeLeadStatements = 2;

    public GroundBassPlan? CreatePlan()
    {
        var feasibleGrounds = GroundBassFeasibilityAnalyzer.GetFeasibleRenderedGrounds(
            compositionConfiguration.InstrumentConfigurations,
            compositionConfiguration.Scale
        );

        if (compositionConfiguration.GroundBassConfiguration?.Pattern is { } configuredPattern)
        {
            feasibleGrounds = feasibleGrounds
                .Where(feasibleGround => feasibleGround.Pattern.Identifier == configuredPattern)
                .ToList();
        }

        if (feasibleGrounds.Count == 0)
        {
            return null;
        }

        var bassInstrument = compositionConfiguration.Instruments[^1];
        var (chosenPattern, chosenBassNotes) = feasibleGrounds[randomProvider.Next(feasibleGrounds.Count)];
        var measuresPerStatement = chosenPattern.ScaleStepOffsets.Count * GroundBassPlan.SlotsPerGroundNote / compositionConfiguration.BeatsPerMeasure;
        var statementCount = Math.Max(
            MinStatementCount,
            (compositionConfiguration.MinimumMeasures + measuresPerStatement - 1) / measuresPerStatement
        );

        var sections = BuildTonalSections(chosenPattern, chosenBassNotes, statementCount);

        return new GroundBassPlan(chosenPattern, bassInstrument, chosenBassNotes, statementCount, measuresPerStatement, sections);
    }

    private List<TonalSection> BuildTonalSections(GroundBassPattern pattern, IReadOnlyList<Note> homeBassNotes, int statementCount)
    {
        var homeOnlySections = new List<TonalSection>
        {
            new(compositionConfiguration.Tonic, compositionConfiguration.Mode, FirstStatement: 0, statementCount - 1, homeBassNotes)
        };

        if (!ShouldAttemptModulation(statementCount))
        {
            return homeOnlySections;
        }

        var (foreignTonic, foreignMode) = compositionConfiguration.Mode == Mode.Ionian
            ? (compositionConfiguration.Scale.Submediant, Mode.Aeolian)
            : (compositionConfiguration.Scale.Mediant, Mode.Ionian);

        var foreignBassNotes = GroundBassFeasibilityAnalyzer.GetFeasibleRenderedGrounds(
                compositionConfiguration.InstrumentConfigurations,
                new BaroquenScale(foreignTonic, foreignMode)
            )
            .Where(feasibleGround => feasibleGround.Pattern.Identifier == pattern.Identifier)
            .Select(static feasibleGround => feasibleGround.BassNotes)
            .FirstOrDefault();

        if (foreignBassNotes is null
            || !IsSeamMotionSingable(homeBassNotes[^1], foreignBassNotes[0])
            || !IsSeamMotionSingable(foreignBassNotes[^1], homeBassNotes[0]))
        {
            return homeOnlySections;
        }

        // The block sits mid-composition by formula, never by draw: about a third of the statements go
        // foreign, placed centrally, with the lead floor keeping the announcement plus one accompanied
        // statement home and the arithmetic guaranteeing at least one home statement before the close.
        var foreignStatementCount = Math.Clamp(statementCount / 3, 1, statementCount - 3);
        var firstForeignStatement = Math.Max(MinHomeLeadStatements, (statementCount - foreignStatementCount) / 2);
        var lastForeignStatement = firstForeignStatement + foreignStatementCount - 1;

        return
        [
            new TonalSection(compositionConfiguration.Tonic, compositionConfiguration.Mode, FirstStatement: 0, firstForeignStatement - 1, homeBassNotes),
            new TonalSection(foreignTonic, foreignMode, firstForeignStatement, lastForeignStatement, foreignBassNotes),
            new TonalSection(compositionConfiguration.Tonic, compositionConfiguration.Mode, lastForeignStatement + 1, statementCount - 1, homeBassNotes)
        ];
    }

    private bool ShouldAttemptModulation(int statementCount) =>
        (compositionConfiguration.GroundBassConfiguration ?? GroundBassConfiguration.Default).Modulate
        && compositionConfiguration.Mode is Mode.Ionian or Mode.Aeolian
        && statementCount >= MinStatementCountForModulation;

    // The seam motions are the two bass steps no walk can dodge: the ground is pinned on both sides of a
    // section boundary, so a seam interval the melodic net rejects (a dissonant leap) would starve every
    // candidate at the seam onset. Relative scales share one note list, so the home scale indexes both notes.
    private bool IsSeamMotionSingable(Note sourceNote, Note targetNote)
    {
        var scaleNotes = compositionConfiguration.Scale.GetNotes();
        var stepDistance = Math.Abs(scaleNotes.IndexOf(sourceNote) - scaleNotes.IndexOf(targetNote));

        return stepDistance <= 1 || !sourceNote.IsDissonantWith(targetNote);
    }
}

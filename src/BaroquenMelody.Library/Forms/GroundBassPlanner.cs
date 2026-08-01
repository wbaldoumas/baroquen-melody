using BaroquenMelody.Infrastructure.Random;
using BaroquenMelody.Library.Configurations;

namespace BaroquenMelody.Library.Forms;

/// <inheritdoc cref="IGroundBassPlanner"/>
/// <remarks>
///     Feasibility lives in <see cref="GroundBassFeasibilityAnalyzer"/> (the same scan the UI runs to warn
///     about shrinking repertoires); the planner draws once among the feasible patterns in bank order, so
///     plan creation costs exactly one draw whenever any ground fits. A configured pattern narrows the draw
///     to itself when it fits (still one draw, keeping the stream aligned across selections); a configured
///     pattern the range cannot host yields no plan, falling back to the fugue the way an empty bank does.
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

        return new GroundBassPlan(chosenPattern, bassInstrument, chosenBassNotes, statementCount, measuresPerStatement);
    }
}

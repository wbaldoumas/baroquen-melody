namespace BaroquenMelody.Library.Forms;

/// <summary>
///     Plans a ground bass composition from the configuration: which pattern, in which register, stated how
///     many times.
/// </summary>
internal interface IGroundBassPlanner
{
    /// <summary>
    ///     Creates the plan, or returns <see langword="null"/> when no pattern in the bank can anchor its
    ///     tonic inside the bass instrument's range, in which case the caller falls back to the standard form.
    /// </summary>
    /// <returns>The plan, or <see langword="null"/> when the bass range cannot host any ground.</returns>
    public GroundBassPlan? CreatePlan();
}

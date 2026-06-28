using BaroquenMelody.Library.Motifs.Enums;

namespace BaroquenMelody.Library.Configurations;

/// <summary>
///     Configures how recurring themes/phrases are developed (transformed) rather than repeated verbatim.
/// </summary>
/// <param name="Enabled"> Whether motivic development is applied at all. When <see langword="false"/>, every restatement is verbatim. </param>
/// <param name="AllowedTransforms"> The ordered repertoire of transforms a restatement may draw from. An empty repertoire degrades to verbatim. </param>
/// <param name="DevelopmentProbability"> The probability (0-100) that an eligible restatement is developed rather than repeated verbatim. </param>
/// <param name="Scope"> How widely a developed restatement is applied across a phrase's voices. </param>
public sealed record MotifDevelopmentConfiguration(
    bool Enabled,
    IList<MotifTransform> AllowedTransforms,
    int DevelopmentProbability,
    MotifDevelopmentScope Scope)
{
    public static MotifDevelopmentConfiguration Default { get; } = new(
        Enabled: true,
        AllowedTransforms: [MotifTransform.Invert, MotifTransform.Retrograde],
        DevelopmentProbability: 50,
        Scope: MotifDevelopmentScope.SingleVoice);
}

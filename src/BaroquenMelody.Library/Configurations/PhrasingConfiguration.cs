namespace BaroquenMelody.Library.Configurations;

/// <summary>
/// Configures phrasing for the composition.
/// </summary>
/// <param name="PhraseLengths"> The lengths of phrases to be used in the composition. </param>
/// <param name="MaxPhraseRepetitions"> The maximum number of times a phrase can be repeated in the composition. </param>
/// <param name="MinPhraseRepetitionPoolSize"> The minimum number of phrases that must be available for repetition in the composition. </param>
/// <param name="PhraseRepetitionProbability"> The probability that a phrase will be repeated in the composition. </param>
public sealed record PhrasingConfiguration(
    IList<int> PhraseLengths,
    int MaxPhraseRepetitions = 4,
    int MinPhraseRepetitionPoolSize = 4,
    int PhraseRepetitionProbability = 100
)
{
    public static PhrasingConfiguration Default => new(PhraseLengths: [2, 4, 8]);

    public int MinPhraseLength { get; } = PhraseLengths.Min();
}

namespace BaroquenMelody.Library.Compositions.Configurations;

/// <summary>
/// Configures phrasing for the composition.
/// </summary>
/// <param name="PhraseLengths"> The lengths of phrases to be used in the composition. </param>
/// <param name="MaxPhraseRepetitions"> The maximum number of times a phrase can be repeated in the composition. </param>
/// <param name="MinPhraseRepetitionPoolSize"> The minimum number of phrases that must be available for repetition in the composition. </param>
/// <param name="PhraseRepetitionProbability"> The probability that a phrase will be repeated in the composition. </param>
internal sealed record PhrasingConfiguration(
    IList<int> PhraseLengths,
    int MaxPhraseRepetitions = 4,
    int MinPhraseRepetitionPoolSize = 2,
    int PhraseRepetitionProbability = 75
)
{
    public static PhrasingConfiguration Default => new(PhraseLengths: [1, 2, 4]);

    public int MinPhraseLength { get; } = PhraseLengths.Min();
}

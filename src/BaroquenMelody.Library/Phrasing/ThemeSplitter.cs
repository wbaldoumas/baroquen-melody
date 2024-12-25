using BaroquenMelody.Library.Domain;

namespace BaroquenMelody.Library.Phrasing;

/// <inheritdoc cref="IThemeSplitter"/>
internal sealed class ThemeSplitter : IThemeSplitter
{
    private static readonly List<(int PhraseLength, int MaxStartIndex)> PhraseSplits =
    [
        (2, 2),
        (4, 0)
    ];

    public List<RepeatedPhrase> SplitThemeIntoPhrases(BaroquenTheme theme)
    {
        var themePhrasesToRepeat = theme.Recapitulation
            .Select(static measure => new RepeatedPhrase { Phrase = [new Measure(measure)] })
            .ToList();

        foreach (var (phraseLength, maxStartIndex) in PhraseSplits)
        {
            if (theme.Recapitulation.Count < phraseLength)
            {
                continue;
            }

            for (var startIndex = 0; startIndex <= Math.Min(maxStartIndex, theme.Recapitulation.Count - phraseLength); startIndex++)
            {
                var endIndex = startIndex + phraseLength;

                themePhrasesToRepeat.Add(
                    new RepeatedPhrase
                    {
                        Phrase = theme.Recapitulation[startIndex..endIndex].Select(static measure => new Measure(measure)).ToList()
                    }
                );
            }
        }

        return themePhrasesToRepeat;
    }
}

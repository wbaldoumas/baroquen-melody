﻿using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Phrasing;

/// <inheritdoc cref="IThemeSplitter"/>
internal sealed class ThemeSplitter : IThemeSplitter
{
    private static readonly List<(int PhraseLength, int MaxStartIndex)> _phraseSplits =
    [
        (2, 2),
        (3, 1),
        (4, 0)
    ];

    public List<RepeatedPhrase> SplitThemeIntoPhrases(BaroquenTheme theme)
    {
        var themePhrasesToRepeat = theme.Recapitulation
            .Select(measure => new RepeatedPhrase { Phrase = [new Measure(measure)] })
            .ToList();

        foreach (var (phraseLength, maxStartIndex) in _phraseSplits)
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
                        Phrase = theme.Recapitulation[startIndex..endIndex].Select(measure => new Measure(measure)).ToList()
                    }
                );
            }
        }

        return themePhrasesToRepeat;
    }
}

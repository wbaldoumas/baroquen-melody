﻿using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Compositions.Extensions;

namespace BaroquenMelody.Library.Compositions.Ornamentation.Cleaning.Engine.Processors;

internal sealed class ThirtySecondNoteOrnamentationCleaner : IProcessor<OrnamentationCleaningItem>
{
    private static readonly int[] IndicesToCheck = [1, 3, 5];

    public void Process(OrnamentationCleaningItem item)
    {
        if (!IndicesToCheck.Any(index => item.Note.Ornamentations[index].IsDissonantWith(item.OtherNote.Ornamentations[index])))
        {
            return;
        }

        if (item.Note > item.OtherNote)
        {
            item.OtherNote.ResetOrnamentation();
        }
        else
        {
            item.Note.ResetOrnamentation();
        }
    }
}

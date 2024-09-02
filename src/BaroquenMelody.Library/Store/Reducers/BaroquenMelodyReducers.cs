﻿using BaroquenMelody.Library.Store.Actions;
using BaroquenMelody.Library.Store.State;
using Fluxor;

namespace BaroquenMelody.Library.Store.Reducers;

public static class BaroquenMelodyReducers
{
    [ReducerMethod]
    public static BaroquenMelodyState ReduceUpdateBaroquenMelody(BaroquenMelodyState state, UpdateBaroquenMelody action) => new(action.BaroquenMelody);
}

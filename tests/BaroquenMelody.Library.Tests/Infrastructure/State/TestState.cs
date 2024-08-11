﻿using Fluxor;

namespace BaroquenMelody.Library.Tests.Infrastructure.State;

internal sealed class TestState : IState<int>
{
    public event EventHandler? StateChanged;

    public int Value { get; private set; }

    public void SetValue(int value)
    {
        Value = value;

        StateChanged?.Invoke(this, EventArgs.Empty);
    }
}

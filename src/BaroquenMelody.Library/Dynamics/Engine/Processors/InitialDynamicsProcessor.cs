﻿using Atrea.PolicyEngine.Processors;
using BaroquenMelody.Library.Configurations;
using Melanchall.DryWetMidi.Common;

namespace BaroquenMelody.Library.Dynamics.Engine.Processors;

internal sealed class InitialDynamicsProcessor(CompositionConfiguration configuration, double velocityRangeTarget = 0.75) : IProcessor<DynamicsApplicationItem>
{
    public void Process(DynamicsApplicationItem item)
    {
        var instrumentConfiguration = configuration.InstrumentConfigurationsByInstrument[item.Instrument];
        var note = item.CurrentBeat[item.Instrument];

        double velocity;

        if (instrumentConfiguration.HasSizeableVelocityRange)
        {
            velocity = instrumentConfiguration.MinVelocity + instrumentConfiguration.VelocityRange * velocityRangeTarget;
        }
        else
        {
            velocity = instrumentConfiguration.MaxVelocity;
        }

        note.Velocity = new SevenBitNumber(Convert.ToByte(velocity));

        foreach (var ornamentation in note.Ornamentations)
        {
            ornamentation.Velocity = note.Velocity;
        }
    }
}
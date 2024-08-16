﻿using BaroquenMelody.Library.Compositions.Enums;

namespace BaroquenMelody.Library.Compositions.Configurations.Services;

/// <summary>
///     A service that deals with the configuration of instruments.
/// </summary>
public interface IInstrumentConfigurationService
{
    /// <summary>
    ///     The instruments that can be configured by the user.
    /// </summary>
    IEnumerable<Instrument> ConfigurableInstruments { get; }

    /// <summary>
    ///     Configure the default instrument configurations.
    /// </summary>
    void ConfigureDefaults();
}

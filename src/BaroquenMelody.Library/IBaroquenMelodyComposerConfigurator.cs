using BaroquenMelody.Library.Composers;
using BaroquenMelody.Library.Configurations;

namespace BaroquenMelody.Library;

/// <summary>
///     Configures a new <see cref="IBaroquenMelodyComposer"/> which can compose a <see cref="BaroquenMelody"/> for the given <see cref="CompositionConfiguration"/>.
/// </summary>
public interface IBaroquenMelodyComposerConfigurator
{
    /// <summary>
    ///     Configure a new <see cref="IBaroquenMelodyComposer"/> with the given <see cref="CompositionConfiguration"/>.
    /// </summary>
    /// <param name="compositionConfiguration">The <see cref="CompositionConfiguration"/> to configure the <see cref="IBaroquenMelodyComposer"/> with.</param>
    /// <returns>The configured <see cref="IBaroquenMelodyComposer"/>.</returns>
    IBaroquenMelodyComposer Configure(CompositionConfiguration compositionConfiguration);
}

using BaroquenMelody.Library.Compositions.Domain;

namespace BaroquenMelody.Library.Compositions.Composers;

internal interface IComposer
{
    Composition Compose();
}

using Fluxor;
using System.Reactive.Linq;

namespace BaroquenMelody.Infrastructure.State;

/// <summary>
///     A home for <see cref="IState{TState}"/> extension methods.
/// </summary>
public static class StateExtensions
{
    /// <summary>
    ///     Create an observable that emits the current state whenever it has changed.
    /// </summary>
    /// <typeparam name="TState">The type of the state to observe.</typeparam>
    /// <param name="state">The state to observe.</param>
    /// <returns>An observable that emits the current state whenever it has changed.</returns>
    public static IObservable<IState<TState>> ObserveChanges<TState>(this IState<TState> state) => Observable.FromEventPattern<EventHandler, EventArgs>(
            unused => state.StateChanged += unused,
            unused => state.StateChanged -= unused
        )
        .Select(_ => state);
}

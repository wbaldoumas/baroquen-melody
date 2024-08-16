using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.App.Components.Extensions;

[ExcludeFromCodeCoverage(Justification = "Simple container configuration")]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBaroquenMelodyComponents(this IServiceCollection services) => services.AddSingleton<IThemeProvider, ThemeProvider>();
}

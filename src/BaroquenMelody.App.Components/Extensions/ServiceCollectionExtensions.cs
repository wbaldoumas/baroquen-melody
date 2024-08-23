using BaroquenMelody.Library.Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using MudExtensions.Services;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.App.Components.Extensions;

[ExcludeFromCodeCoverage(Justification = "Simple container configuration")]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBaroquenMelodyComponents(this IServiceCollection services) => services
        .AddMudServices()
        .AddMudExtensions()
        .AddBaroquenMelody()
        .AddSingleton<IThemeProvider, ThemeProvider>();
}

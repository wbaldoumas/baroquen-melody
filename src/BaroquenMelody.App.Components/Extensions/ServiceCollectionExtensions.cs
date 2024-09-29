using BaroquenMelody.Library.Extensions;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using MudExtensions.Services;
using System.Diagnostics.CodeAnalysis;

namespace BaroquenMelody.App.Components.Extensions;

[ExcludeFromCodeCoverage(Justification = "Simple container configuration")]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBaroquenMelodyComponents(this IServiceCollection services) => services
        .AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomCenter;
            config.SnackbarConfiguration.SnackbarVariant = Variant.Outlined;
        })
        .AddMudExtensions()
        .AddBaroquenMelody();
}

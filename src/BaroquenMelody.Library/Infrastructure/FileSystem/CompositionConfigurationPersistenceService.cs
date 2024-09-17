using BaroquenMelody.Library.Compositions.Configurations;
using BaroquenMelody.Library.Infrastructure.Logging;
using BaroquenMelody.Library.Infrastructure.Serialization.JsonSerializerContexts;
using Microsoft.Extensions.Logging;
using System.IO.Abstractions;
using System.Text.Json;

namespace BaroquenMelody.Library.Infrastructure.FileSystem;

internal sealed class CompositionConfigurationPersistenceService(
    IDeviceDirectoryProvider deviceDirectoryProvider,
    IDirectory directory,
    IFile file,
    IFileSystem fileSystem,
    ILogger<BaroquenMelody> logger
) : ICompositionConfigurationPersistenceService
{
    public async Task<bool> SaveConfigurationAsync(CompositionConfiguration compositionConfiguration, string name, CancellationToken cancellationToken)
    {
        try
        {
            var serializedCompositionConfiguration = JsonSerializer.Serialize(
                compositionConfiguration,
                CompositionConfigurationJsonSerializerContext.Default.CompositionConfiguration
            );

            var filePath = Path.Combine(deviceDirectoryProvider.AppDataDirectory, $"{name}.dat");
            var fileStream = fileSystem.FileStream.New(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);

            await using (fileStream.ConfigureAwait(false))
            {
                var binaryWriter = new BinaryWriter(fileStream);

                await using (binaryWriter.ConfigureAwait(false))
                {
                    binaryWriter.Write(serializedCompositionConfiguration);
                }
            }

            return true;
        }
        catch (Exception exception)
        {
            logger.LogException("Failed to save composition configuration", exception.GetType().ToString(), exception.Message, exception.StackTrace);

            return false;
        }
    }

    public async Task<IEnumerable<SavedCompositionConfiguration>> LoadConfigurationsAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!directory.Exists(deviceDirectoryProvider.AppDataDirectory))
            {
                return [];
            }

            var loadConfigurationTasks = directory
                .EnumerateFiles(deviceDirectoryProvider.AppDataDirectory)
                .Where(configurationFile => configurationFile.EndsWith(".dat", StringComparison.OrdinalIgnoreCase))
                .Select(configurationFile => new FileInfo(configurationFile))
                .OrderByDescending(fileInfo => fileInfo.CreationTime)
                .Select(async fileInfo =>
                {
                    var configuration = await LoadCompositionConfiguration(fileInfo.FullName).ConfigureAwait(false);

                    return new SavedCompositionConfiguration(configuration, fileInfo);
                });

            var configurations = await Task.WhenAll(loadConfigurationTasks).ConfigureAwait(false);

            return configurations;
        }
        catch (Exception exception)
        {
            logger.LogException("Failed to load composition configurations", exception.GetType().ToString(), exception.Message, exception.StackTrace);

            throw;
        }
    }

    public Task<bool> DeleteConfigurationAsync(string name, CancellationToken cancellationToken)
    {
        try
        {
            var filePath = Path.Combine(deviceDirectoryProvider.AppDataDirectory, name);

            if (file.Exists(filePath))
            {
                file.Delete(filePath);
            }

            return Task.FromResult(true);
        }
        catch (Exception exception)
        {
            logger.LogException("Failed to delete composition configuration", exception.GetType().ToString(), exception.Message, exception.StackTrace);

            return Task.FromResult(false);
        }
    }

    public Task<bool> DoesConfigurationExist(string name, CancellationToken cancellationToken) => Task.FromResult(
        file.Exists(
            Path.Combine(
                deviceDirectoryProvider.AppDataDirectory,
                $"{name}.dat"
            )
        )
    );

    private async Task<CompositionConfiguration> LoadCompositionConfiguration(string name)
    {
        var fileStream = fileSystem.FileStream.New(name, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);

        await using (fileStream.ConfigureAwait(false))
        {
            using var binaryReader = new BinaryReader(fileStream);
            var serializedCompositionConfiguration = binaryReader.ReadString();

            return JsonSerializer.Deserialize(
                serializedCompositionConfiguration,
                CompositionConfigurationJsonSerializerContext.Default.CompositionConfiguration
            )!;
        }
    }
}

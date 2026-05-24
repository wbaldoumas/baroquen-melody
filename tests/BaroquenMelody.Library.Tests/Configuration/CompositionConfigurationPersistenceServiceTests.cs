using BaroquenMelody.Infrastructure.Devices;
using BaroquenMelody.Library.Configurations.Services;
using BaroquenMelody.Library.Tests.TestData;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;

namespace BaroquenMelody.Library.Tests.Configuration;

[TestFixture]
internal sealed class CompositionConfigurationPersistenceServiceTests
{
    private IDeviceDirectoryProvider _mockDeviceDirectoryProvider = null!;

    private IDirectory _mockDirectory = null!;

    private IFile _mockFile = null!;

    private IFileSystem _mockFileSystem = null!;

    private ILogger<MidiFileComposition> _mockLogger = null!;

    private CompositionConfigurationPersistenceService _persistenceService = null!;

    [SetUp]
    public void SetUp()
    {
        _mockDeviceDirectoryProvider = Substitute.For<IDeviceDirectoryProvider>();
        _mockDirectory = Substitute.For<IDirectory>();
        _mockFile = Substitute.For<IFile>();
        _mockFileSystem = Substitute.For<IFileSystem>();
        _mockLogger = Substitute.For<ILogger<MidiFileComposition>>();

        _persistenceService = new CompositionConfigurationPersistenceService(
            _mockDeviceDirectoryProvider,
            _mockDirectory,
            _mockFile,
            _mockFileSystem,
            _mockLogger
        );
    }

    [Test]
    public async Task SaveConfigurationAsync_WhenSucceeds_ReturnsTrue()
    {
        // act
        _mockFileSystem.FileStream.New(
                Arg.Any<string>(),
                Arg.Any<FileMode>(),
                Arg.Any<FileAccess>(),
                Arg.Any<FileShare>(),
                Arg.Any<int>(),
                Arg.Any<bool>()
            )
            .Returns(
                new MockFileStream(new MockFileSystem(new MockFileSystemOptions()), "tests", FileMode.Create)
            );

        var result = await _persistenceService.SaveConfigurationAsync(
            TestCompositionConfigurations.Get(),
            "test",
            CancellationToken.None
        );

        // assert
        result.Should().BeTrue();
    }

    [Test]
    public async Task SaveConfigurationAsync_WhenFails_ReturnsFalse()
    {
        // arrange
        _mockFileSystem.FileStream.Throws(new InvalidOperationException());

        // act
        var result = await _persistenceService.SaveConfigurationAsync(
            TestCompositionConfigurations.Get(),
            "test",
            CancellationToken.None
        );

        // assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task LoadConfigurationsAsync_WhenFails_Throws()
    {
        // arrange
        _mockDeviceDirectoryProvider.AppDataDirectory.Returns("test-dir");
        _mockDirectory.Exists(Arg.Any<string>()).Returns(true);
        _mockDirectory.EnumerateFiles(Arg.Any<string>()).Throws(new InvalidOperationException());

        // act
        var act = () => _persistenceService.LoadConfigurationsAsync(CancellationToken.None);

        // assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public async Task DeleteConfigurationAsync_WhenFails_ReturnsFalse()
    {
        // arrange
        _mockDeviceDirectoryProvider.AppDataDirectory.Returns("test-dir");
        _mockFile.Exists(Arg.Any<string>()).Throws(new InvalidOperationException());

        // act
        var result = await _persistenceService.DeleteConfigurationAsync("test", CancellationToken.None);

        // assert
        result.Should().BeFalse();
    }
}

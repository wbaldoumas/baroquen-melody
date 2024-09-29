using BaroquenMelody.Infrastructure.Devices;
using BaroquenMelody.Library.Compositions.Configurations.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using TestDataConfigurations = BaroquenMelody.Library.Tests.TestData.Configurations;

namespace BaroquenMelody.Library.Tests.Compositions.Configuration;

[TestFixture]
internal sealed class CompositionConfigurationPersistenceServiceTests
{
    private IDeviceDirectoryProvider _mockDeviceDirectoryProvider = null!;

    private IDirectory _mockDirectory = null!;

    private IFile _mockFile = null!;

    private IFileSystem _mockFileSystem = null!;

    private ILogger<BaroquenMelody> _mockLogger = null!;

    private CompositionConfigurationPersistenceService _persistenceService = null!;

    [SetUp]
    public void SetUp()
    {
        _mockDeviceDirectoryProvider = Substitute.For<IDeviceDirectoryProvider>();
        _mockDirectory = Substitute.For<IDirectory>();
        _mockFile = Substitute.For<IFile>();
        _mockFileSystem = Substitute.For<IFileSystem>();
        _mockLogger = Substitute.For<ILogger<BaroquenMelody>>();

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
            TestDataConfigurations.GetCompositionConfiguration(),
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
            TestDataConfigurations.GetCompositionConfiguration(),
            "test",
            CancellationToken.None
        );

        // assert
        result.Should().BeFalse();
    }
}

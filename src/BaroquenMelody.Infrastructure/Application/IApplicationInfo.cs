namespace BaroquenMelody.Infrastructure.Application;

/// <summary>
///     An abstraction for providing application information.
/// </summary>
public interface IApplicationInfo
{
    /// <summary>
    ///     The application version.
    /// </summary>
    public string Version { get; }

    /// <summary>
    ///     The application build number.
    /// </summary>
    public string Build { get; }

    /// <summary>
    ///     The short commit hash.
    /// </summary>
    public string Commit { get; }

    /// <summary>
    ///     The commit date.
    /// </summary>
    public string CommitDate { get; }

    /// <summary>
    ///     The full commit hash.
    /// </summary>
    public string Sha { get; }

    /// <summary>
    ///     The repository URL.
    /// </summary>
    public string RepositoryUrl { get; }

    /// <summary>
    ///     The support URL.
    /// </summary>
    public string SupportUrl { get; }

    /// <summary>
    ///      The contribution URL.
    /// </summary>
    public string ContributeUrl { get; }

    /// <summary>
    ///     The license URL.
    /// </summary>
    public string LicenseUrl { get; }
}

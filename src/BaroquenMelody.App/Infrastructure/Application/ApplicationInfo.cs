using BaroquenMelody.Infrastructure.Application;
using System.Globalization;

namespace BaroquenMelody.App.Infrastructure.Application;

internal sealed class ApplicationInfo(IAppInfo appInfo) : IApplicationInfo
{
    public string Version => appInfo.Version.ToString(3);

    public string Build => appInfo.BuildString;

    public string Commit => ThisAssembly.Git.Commit;

    public string CommitDate => DateTime.Parse(ThisAssembly.Git.CommitDate, CultureInfo.InvariantCulture).ToString("dd-MMM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);

    public string Sha => ThisAssembly.Git.Sha;

    public string RepositoryUrl => "https://github.com/wbaldoumas/baroquen-melody";

    public string SupportUrl => "https://buymeacoffee.com/baroquenmelody";

    public string ContributeUrl => "https://github.com/wbaldoumas/baroquen-melody/blob/main/CONTRIBUTING.md";

    public string LicenseUrl => "https://github.com/wbaldoumas/baroquen-melody/blob/main/LICENSE";
}

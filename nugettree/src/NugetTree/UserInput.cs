
namespace NugetTree
{
    using NuGet;
    using System.Collections.Generic;
    using System.Runtime.Versioning;

    public class UserInput
    {
        public string RepoFolder { get; set; } = string.Empty;

        public string TargetFramework { get; set; } = string.Empty;

        public string PackageSource { get; set; } = string.Empty;

        public FrameworkName FrameworkName { get; set; } = null;

        public IPackageRepository NugetRepoFactory { get; set; }

        public List<PackageSummaries> Dependencies { get; set; }
    }
}

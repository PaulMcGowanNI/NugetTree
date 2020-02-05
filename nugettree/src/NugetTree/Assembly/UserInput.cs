
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
    }
}

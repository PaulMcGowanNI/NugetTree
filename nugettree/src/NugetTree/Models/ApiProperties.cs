
namespace NugetTree
{
    using NuGet;
    using System.Collections.Generic;
    using System.Runtime.Versioning;

    public class ApiProperties
    {
        public FrameworkName FrameworkName { get; set; } = null;
        public IPackageRepository NugetOldFactory { get; set; }
        public NuGet.Configuration.PackageSource NugetPackageSource { get; set; }
        public List<PackageSummaries> Dependencies { get; set; }
    }
}

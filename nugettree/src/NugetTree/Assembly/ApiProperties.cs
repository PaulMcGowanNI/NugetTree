
namespace NugetTree
{
    using NuGet;
    using NuGet.Protocol.Core.Types;
    using System.Collections.Generic;
    using System.Runtime.Versioning;
    public class ApiProperties
    {
        public FrameworkName FrameworkName { get; set; } = null;
        public IPackageRepository NugetOldFactory { get; set; }
        public IPackageSearchMetadata NugetNewFactory { get; set; }
        public NuGet.Configuration.PackageSource NugetPackageSource { get; set; }
        public List<PackageSummaries> Dependencies { get; set; }
    }
}

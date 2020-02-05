using NuGet;
using System.Collections.Generic;

namespace NugetTree
{
    public class PackageSummaries
    {
        public string Project { get; set; }

        public List<IPackage> Packages { get; set; }

        public List<IPackage> LatestVersion { get; set; }
    }
}

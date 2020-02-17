using NuGet;
using NuGet.Protocol.Core.Types;
using NugetTree.Assembly;
using System.Collections.Generic;

namespace NugetTree
{
    public class PackageSummaries
    {
        public string Project { get; set; }

        public List<IPackage> Packages { get; set; }

        public List<IPackage> LatestVersion { get; set; }

        public List<IPackageSearchMetadata> LatestVersionMetaData { get; set; }

        public List<LocalPackageDetails> LocalVersionMetaData { get; set; }
    }
}

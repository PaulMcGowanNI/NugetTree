
namespace NugetTree
{
    using NuGet;
    using NuGet.Protocol.Core.Types;
    using NugetTree.Services;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Xml;

    public static class PackageConfiguration
    {
        public static List<PackageReference> FindPackageConfigDependencies(string folder)
        {
            var disPackages = new List<PackageReference>();
            var packageConfigFiles = Directory.EnumerateFiles(folder, "packages.config", SearchOption.AllDirectories);

            foreach (var packageConfig in packageConfigFiles)
            {
                var packages = new PackageReferenceFile(packageConfig).GetPackageReferences();

                disPackages.AddRange(packages);
            }

            return disPackages.Distinct().ToList();
        }

        public static List<Tuple<string, SemanticVersion>> FindProjectFilesDependencies(string folder)
        {
            var nugetReferences = new List<Tuple<string, SemanticVersion>>();
            var projectFiles = Directory.EnumerateFiles(folder, "*.csproj", SearchOption.AllDirectories);

            foreach (var projectFile in projectFiles)
            {
                try
                {
                    var xmldoc = new XmlDocument();
                    xmldoc.Load(projectFile);

                    foreach (XmlNode item in xmldoc.SelectNodes("/Project/ItemGroup"))
                    {
                        nugetReferences.Add(new Tuple<string, SemanticVersion>(item.Attributes["Include"].Value.ToString(), new SemanticVersion(item.Attributes["Version"].Value.ToString())));
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Error - {ex.Message}");
                }
            }
            return nugetReferences.ToList();
        }

        public static IEnumerable<IPackageSearchMetadata> GetPackageVersions(PackageReference reference, NuGet.Configuration.PackageSource nugetPackageSource)
        {
            Logger logger = new Logger();
            List<Lazy<INuGetResourceProvider>> providers = new List<Lazy<INuGetResourceProvider>>();
            providers.AddRange(Repository.Provider.GetCoreV3());  // Add v3 API support

            SourceRepository sourceRepository = new SourceRepository(nugetPackageSource, providers);

            PackageMetadataResource packageMetadataResource = sourceRepository.GetResourceAsync<PackageMetadataResource>().Result;
            SourceCacheContext sourceCacheContext = new SourceCacheContext();

            IEnumerable<IPackageSearchMetadata> ExactsearchMetadata = packageMetadataResource
                      .GetMetadataAsync(reference.Id?.Trim(), true, true, sourceCacheContext, logger, CancellationToken.None).Result;
            return ExactsearchMetadata;
        }
    }
}

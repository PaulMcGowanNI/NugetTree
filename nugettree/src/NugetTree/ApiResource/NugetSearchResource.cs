
namespace NugetTree.ApiResource
{
    using NuGet;
    using NuGet.Packaging.Core;
    using NuGet.Protocol.Core.Types;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class NugetSearchResource
    {
        private UserInput _userInput;
        private ApiProperties _apiProperties;

        public NugetSearchResource(UserInput userInput, ApiProperties apiProperties)
        {
            _userInput = userInput;
            _apiProperties = apiProperties;
        }

        public List<PackageSummaries> ListAll()
        {
            return SearchDependency(true);
        }

        private List<PackageSummaries> SearchDependency(bool showEmptyResults)
        {
            var solutionFolders = Directory.EnumerateDirectories(_userInput.RepoFolder).Where(x => !x.StartsWith(".") && !x.EndsWith("\\.git") && !x.EndsWith("\\.vs"));

            if (!solutionFolders.Any(x => !x.EndsWith("\\bin") && !x.EndsWith("\\obj")))
            {
                solutionFolders = new[] { _userInput.RepoFolder };
            }

            GatherAPIData(solutionFolders);

            return new List<PackageSummaries>();
        }

        private void GatherAPIData(IEnumerable<string> solutionFolders)
        {
            foreach (var folder in solutionFolders)
            {
                try
                {
                    var folderName = Path.GetFileName(folder);

                    List<PackageReference> localPackages = PackageConfiguration.FindPackageConfigDependencies(folder);
                    List<Tuple<string, SemanticVersion>> currentPackages = PackageConfiguration.FindProjectFilesDependencies(folder);

                    foreach (var current in localPackages)
                    {
                        //--------------------------
                        IEnumerable<IPackageSearchMetadata> ExactsearchMetadata = PackageConfiguration.GetPackageVersions(current, _apiProperties.NugetPackageSource);

                        if (ExactsearchMetadata.Any())
                        {
                            var latestPackage = ExactsearchMetadata.OrderByDescending(x => x.Identity.Version)
                               .FirstOrDefault();

                            if (current.Version.ToString() != latestPackage.Identity.Version.ToString())
                            {

                            }

                        }
                        //--------------------------

                    }

                    //if (livePackage.Any())
                    //{
                    //    projects.Add(new PackageSummaries
                    //    {
                    //        Project = folderName,
                    //        Packages = items.Distinct().OrderBy(x => x.Id).ToList(),
                    //        LatestVersion = latestVersionsItems
                    //    });
                    //}
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Folder \"{folder}\" threw an exception. Ex: {ex}");
                }
            }
        }
    }
}

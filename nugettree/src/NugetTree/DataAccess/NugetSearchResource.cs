
namespace NugetTree.ApiResource
{
    using NuGet;
    using NuGet.Protocol.Core.Types;
    using NugetTree.Assembly;
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

            return GatherAPIData(solutionFolders);
        }

        private List<PackageSummaries> GatherAPIData(IEnumerable<string> solutionFolders)
        {
            var projects = new List<PackageSummaries>();
            foreach (var folder in solutionFolders)
            {
                try
                {
                    var isLatestPackageVersin = true;
                    var folderName = Path.GetFileName(folder);
                    var latestVersionNumber = new List<IPackageSearchMetadata>();
                    var currentPackage = new List<LocalPackageDetails>();
                    List<PackageReference> deps1 = PackageConfiguration.FindPackageConfigDependencies(folder);

                    foreach (var current in deps1)
                    {
                        // Get nuget.org live package versions
                        IEnumerable<IPackageSearchMetadata> ExactsearchMetadata = PackageConfiguration.GetPackageVersions(current, _apiProperties.NugetPackageSource);

                        if (ExactsearchMetadata.Any())
                        {
                            // Get nuget.org latest package version
                            var latestPackage = ExactsearchMetadata.OrderByDescending(x => x.Identity.Version)
                               .FirstOrDefault();

                            if (current.Version.ToString() != latestPackage.Identity.Version.ToString())
                            {
                                // Package is not the latest version. Add the latest version to a list
                                latestVersionNumber.Add(latestPackage);
                                isLatestPackageVersin = false;
                            }

                            Console.Write("+");
                            currentPackage.Add(new LocalPackageDetails
                            {
                                Id = current.Id,
                                Version = current.Version,
                                IsLatestVersion = isLatestPackageVersin
                            });
                        }
                        else
                        {
                            Console.Write("!");
                        }
                    }

                    if (currentPackage.Any())
                    {
                        projects.Add(new PackageSummaries
                        {
                            Project = folderName,
                            LocalVersionMetaData = currentPackage.Distinct().OrderBy(x => x.Id).ToList(),
                            LatestVersionMetaData = latestVersionNumber
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Folder \"{folder}\" threw an exception. Ex: {ex}");
                }
            }
            return projects;
        }
    }
}

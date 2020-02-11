
namespace NugetTree.ApiResource
{
    using NuGet;
    using NuGet.Protocol.Core.Types;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class NugetSearchResource
    {
        private UserInput _userInput;
        private ApiProperties _apiProperties;
        private IPackageSearchMetadata _nugetSearchRepo;

        public NugetSearchResource(UserInput userInput, ApiProperties apiProperties, IPackageSearchMetadata nugetSearchRepo)
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
            var package = new List<PackageSummaries>();
            var folders = Directory.EnumerateDirectories(_userInput.RepoFolder).Where(x => !x.StartsWith(".") && !x.EndsWith("\\.git") && !x.EndsWith("\\.vs"));

            if (!folders.Any(x => !x.EndsWith("\\bin") && !x.EndsWith("\\obj")))
            {
                folders = new[] { _userInput.RepoFolder };
            }

            GatherAPIData(package, folders);

            return package;
        }

        private void GatherAPIData(List<PackageSummaries> projects, IEnumerable<string> folders)
        {
            foreach (var folder in folders)
            {
                try
                {
                    string folderName = Path.GetFileName(folder);

                    List<PackageSummaries> project = new List<PackageSummaries>();
                    List<PackageReference> configDependencies = PackageConfiguration.FindPackageConfigDependencies(folder);
                    List<Tuple<string, SemanticVersion>> currentPackages = PackageConfiguration.FindProjectFilesDependencies(folder);

                    IPackageSearchMetadata livePackage = null;
                    List<IPackage> items = new List<IPackage>();
                    List<IPackage> latestVersionsItems = new List<IPackage>();

                    foreach (var dep1 in configDependencies)
                    {
                        //--------------------------
                        IEnumerable<IPackageSearchMetadata> ExactsearchMetadata = PackageConfiguration.GetPackageVersions(dep1, _apiProperties.NugetPackageSource);

                        if (ExactsearchMetadata.Any())
                        {
                            livePackage = ExactsearchMetadata.OrderByDescending(x => x.Identity.Version)
                               .FirstOrDefault();
                        }
                        //--------------------------

                        //    var package = _nugetRepo.FindPackage(dep1.Id?.Trim(), dep1.Version);
                        //    if (package != null)
                        //    {
                        //        var livePackages = _nugetRepo.FindPackagesById(dep1.Id?.Trim()).Where(x => x.IsLatestVersion == true).ToList();

                        //        foreach (var version in livePackages)
                        //        {
                        //            if (dep1.Version != version.Version)
                        //            {
                        //                latestVersionsItems.Add(version);
                        //            }
                        //        }

                        //        Console.Write("1");
                        //        items.Add(package);
                        //    }
                        //    else
                        //    {
                        //        Console.Write("0");
                        //    }
                        //}

                        //foreach (var dep2 in currentPackages)
                        //{
                        //    var package = _nugetRepo.FindPackage(dep2.Item1?.Trim(), dep2.Item2);
                        //    if (package != null)
                        //    {
                        //        Console.Write("+");
                        //        items.Add(package);
                        //    }
                        //    else
                        //    {
                        //        Console.Write("!");
                        //    }
                        //}
                    }

                    if (items.Any())
                    {
                        projects.Add(new PackageSummaries
                        {
                            Project = folderName,
                            Packages = items.Distinct().OrderBy(x => x.Id).ToList(),
                            LatestVersion = latestVersionsItems
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Folder \"{folder}\" threw an exception. Ex: {ex}");
                }
            }
        }
    }
}

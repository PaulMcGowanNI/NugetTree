using NuGet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace NugetTree
{
    public class FindDependencies
    {
        private readonly string _localFolderPath;
        private readonly IPackageRepository _nugetRepo;

        public FindDependencies(string localFolderPath, IPackageRepository nugetRepo)
        {
            _localFolderPath = localFolderPath;
            _nugetRepo = nugetRepo;
        }

        public List<PackageSummaries> ListAll()
        {
            return Search(true, string.Empty, string.Empty, string.Empty, false);
        }

        public List<PackageSummaries> Search(string searchContains)
        {
            return Search(true, string.Empty, searchContains, string.Empty, false);
        }

        public List<PackageSummaries> Search(bool showEmptyResults, string searchStartsWith, string searchContains, string searchEndsWith, bool onlyShowInvalidSemanticVersions)
        {
            var projects = new List<PackageSummaries>();
            var folders = Directory.EnumerateDirectories(_localFolderPath).Where(f => !f.StartsWith("."));

            if (!folders.Any(f => !f.EndsWith("\\bin") && !f.EndsWith("\\obj")))
            {
                folders = new[] { _localFolderPath };
            }

            foreach (var folder in folders)
            {
                try
                {

                    var project = new List<PackageSummaries>();
                    var folderName = Path.GetFileName(folder);
                    var deps1 = FindPackageConfigDependencies(folder);
                    var deps2 = FindProjectFilesDependencies(folder);

                    var items = new List<IPackage>();
                    var latestVersionsItems = new List<IPackage>();

                    foreach (var dep1 in deps1)
                    {
                        var package = _nugetRepo.FindPackage(dep1.Id?.Trim(), dep1.Version);
                        if (package != null)
                        {
                            var livePackages = _nugetRepo.FindPackagesById(dep1.Id?.Trim()).Where(x => x.IsLatestVersion == true).ToList();

                            foreach (var version in livePackages)
                            {
                                if (dep1.Version != version.Version)
                                {
                                    latestVersionsItems.Add(version);
                                }
                            }

                            Console.Write("+");
                            items.Add(package);
                        }
                        else
                        {
                            Console.Write("!");
                        }
                    }

                    foreach (var dep2 in deps2)
                    {
                        var package = _nugetRepo.FindPackage(dep2.Item1?.Trim(), dep2.Item2);
                        if (package != null)
                        {
                            Console.Write("+");
                            items.Add(package);
                        }
                        else
                        {
                            Console.Write("!");
                        }
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

            // Apply search criteria
            foreach (var project in projects)
            {
                project.Packages = project.Packages.Where(x => (string.IsNullOrWhiteSpace(searchStartsWith) || x.Id.ToLower().StartsWith(searchStartsWith.ToLower()) || x.Version.ToString().ToLower().StartsWith(searchStartsWith.ToLower()))
                                                                        && (string.IsNullOrWhiteSpace(searchContains) || x.Id.ToLower().Contains(searchContains.ToLower()) || x.Version.ToString().ToLower().Contains(searchContains.ToLower()))
                                                                        && (string.IsNullOrWhiteSpace(searchEndsWith) || x.Id.ToLower().EndsWith(searchEndsWith.ToLower()) || x.Version.ToString().ToLower().EndsWith(searchEndsWith.ToLower()))).ToList();

                if (onlyShowInvalidSemanticVersions)
                {
                    project.Packages = project.Packages.Where(x =>
                    {
                        var versionStringParts = x.Version.ToString().Split(new[] { "." }, StringSplitOptions.RemoveEmptyEntries);

                        if (versionStringParts.Length == 4)
                        {
                            if (versionStringParts[0].StartsWith("0") && versionStringParts[0].Length != 1)
                            {
                                return true;
                            }

                            if (versionStringParts[1].StartsWith("0") && versionStringParts[1].Length != 1)
                            {
                                return true;
                            }

                            if (versionStringParts[2].StartsWith("0") && versionStringParts[2].Length != 1)
                            {
                                return true;
                            }

                            if (versionStringParts[3].StartsWith("0") && versionStringParts[3].Length != 1)
                            {
                                return true;
                            }
                        }

                        return false;
                    }).ToList();
                }
            }

            // Apply hide empty results
            if (!showEmptyResults)
            {
                projects = projects.Where(x => x.Packages != null && x.Packages.Any()).ToList();
            }

            return projects;
        }

        private List<PackageReference> FindPackageConfigDependencies(string folder)
        {
            var capitaPackages = new List<PackageReference>();
            var packageConfigFiles = Directory.EnumerateFiles(folder, "packages.config", SearchOption.AllDirectories);

            foreach (var packageConfig in packageConfigFiles)
            {
                var packages = new PackageReferenceFile(packageConfig).GetPackageReferences();

                capitaPackages.AddRange(packages);
            }

            return capitaPackages.Distinct().ToList();
        }

        private List<Tuple<string, SemanticVersion>> FindProjectFilesDependencies(string folder)
        {
            var nugetReferences = new List<Tuple<string, SemanticVersion>>();
            var projectFiles = Directory.EnumerateFiles(folder, "*.csproj", SearchOption.AllDirectories);

            foreach (var projectFile in projectFiles)
            {
                try
                {
                    var xmldoc = new XmlDocument();
                    xmldoc.Load(projectFile);

                    foreach (XmlNode item in xmldoc.SelectNodes("/Project/ItemGroup/PackageReference"))
                    {
                        nugetReferences.Add(new Tuple<string, SemanticVersion>(item.Attributes["Include"].Value.ToString(), new SemanticVersion(item.Attributes["Version"].Value.ToString())));
                    }
                }
                catch
                {
                }
            }

            return nugetReferences.ToList();
        }
    }
}
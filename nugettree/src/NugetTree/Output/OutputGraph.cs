
namespace NugetTree.Output
{
    using NuGet;
    using NuGet.Protocol.Core.Types;
    using NugetTree.Assembly;
    using NugetTree.Font;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Versioning;

    public class OutputGraph
    {
        public void OutputV3Graph(IEnumerable<LocalPackageDetails>localPackages, FrameworkName targetFramework, IEnumerable<IPackageSearchMetadata> latestVersion, int depth)
        {
            foreach (var package in localPackages)
            {
                if (latestVersion.Any(x => x.Identity.Id == package.Id) && package.IsLatestVersion == false)
                {
                    foreach (var list in latestVersion.Where(x => x.Identity.Id == package.Id))
                    {
                        FontColour.NuGetVersion(list.Identity.Version);
                        Console.WriteLine($"{new string(' ', depth)}{package.Id} v{package.Version} | Latest Package:{package.IsLatestVersion} | {list.Identity.Id} v{list.Identity.Version}");
                        FontColour.NormalColor();
                    }
                }
                else
                {
                    FontColour.NugetColor(package.Version);
                    Console.WriteLine($"{new string(' ', depth)}{package.Id} v{package.Version} | Latest Package:{package.IsLatestVersion}");
                }
            }
        }

        public void OutputV2Graph(IPackageRepository repository, IEnumerable<IPackage> packages, FrameworkName targetFramework, List<IPackage> latestVersion, int depth)
        {
            foreach (IPackage package in packages)
            {
                if (latestVersion.Any(x => x.Id == package.Id) && package.IsLatestVersion == false)
                {
                    foreach (var list in latestVersion.Where(x => x.Id == package.Id))
                    {
                        FontColour.NugetColor(list.Version);
                        Console.WriteLine($"{new string(' ', depth)}{package.Id} v{package.Version} | Latest Package:{package.IsLatestVersion} | {list.Id} v{list.Version}");
                        FontColour.NormalColor();
                    }
                }
                else
                {
                    Console.WriteLine($"{new string(' ', depth)}{package.Id} v{package.Version} Latest Package:{package.IsLatestVersion}");
                }


                IList<IPackage> dependentPackages = new List<IPackage>();

                var matchingPackageDependencySets = package.DependencySets.Where(x => x.SupportedFrameworks.Any(y => y.Identifier == targetFramework.Identifier && y.Version <= targetFramework.Version));

                PackageDependencySet chosenDependencySet = null;
                foreach (var match in matchingPackageDependencySets)
                {
                    if (chosenDependencySet == null || (chosenDependencySet.SupportedFrameworks.Any(x => x.Version > match.SupportedFrameworks.OrderByDescending(v => v.Version).Select(z => z.Version).First())))
                    {
                        chosenDependencySet = match;
                    }
                }

                if (chosenDependencySet != null && chosenDependencySet.Dependencies.Any())
                {
                    foreach (var dependency in chosenDependencySet.Dependencies.OrderBy(x => x.Id))
                    {
                        var dependentPackage = repository.FindPackage(dependency.Id, dependency.VersionSpec, true, true);
                        if (dependentPackage != null)
                        {
                            dependentPackages.Add(dependentPackage);
                        }
                    }

                    OutputV2Graph(repository, dependentPackages, targetFramework, latestVersion, depth + 3);
                }
            }
        }
    }
}

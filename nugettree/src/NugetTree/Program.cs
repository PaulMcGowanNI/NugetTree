using NuGet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;

namespace NugetTree
{
    class Program
    {
        static void Main()
        {
            //Console.Write("Enter the local repo folder: ");
            var repoFolder = @"C:\Repo2\commissionMain\src";
            var targetFramework = new FrameworkName(".NETFramework,Version=v4.6.1");

            var nugetRepo = PackageRepositoryFactory.Default.CreateRepository("http://nuget01.casfs.co.uk/nuget/live/");
            var answer = new FindDependencies(repoFolder, nugetRepo).ListAll();

            foreach (var item in answer)
            {
                Console.WriteLine();
                Console.WriteLine("--------------------------------------");
                Console.WriteLine(item.Project);

                OutputGraph(nugetRepo, item.Packages, targetFramework, item.LatestVersion, 0);
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        private static SemanticVersion fontColor(SemanticVersion version)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            return version;
        }

        private static void normalColor()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        static void OutputGraph(IPackageRepository repository, IEnumerable<IPackage> packages, FrameworkName targetFramework, List<IPackage> latestVersion, int depth)
        {
            foreach (IPackage package in packages)
            {
                if (latestVersion.Any(x => x.Id == package.Id) && package.IsLatestVersion == false)
                {
                    foreach (var list in latestVersion.Where(x => x.Id == package.Id))
                    {
                        fontColor(list.Version);
                        Console.WriteLine("{0}{1} v{2} | Latest Package:{3} | {4} v{5}", new string(' ', depth), package.Id, package.Version, package.IsLatestVersion, list.Id, list.Version);
                        normalColor();
                    }
                }
                else
                {
                    Console.WriteLine("{0}{1} v{2} Latest Package:{3}", new string(' ', depth), package.Id, package.Version, package.IsLatestVersion);
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

                    OutputGraph(repository, dependentPackages, targetFramework, latestVersion, depth + 3);
                }
            }
        }
    }
}

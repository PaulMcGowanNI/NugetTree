
namespace NugetTree
{
    using NuGet;
    using NugetTree.Font;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Versioning;
    class Program
    {
        static void Main()
        {
            #region EnterRepository

            string repoFolder = string.Empty;
            do
            {
                repoFolder = GetSolutionPath(repoFolder);
            } while (string.IsNullOrEmpty(repoFolder));

            #endregion EnterRepository

            #region EnterFramework

            string targetFramework = string.Empty;
            do
            {
                targetFramework = GetFrameworkVersion(targetFramework);
                if (!string.IsNullOrEmpty(targetFramework))
                {
                    var frameworkVersion = new FrameworkName(targetFramework);
                }

            } while (string.IsNullOrEmpty(targetFramework));

            #endregion EnterFramework

            #region EnterPackageSource

            string nugetRepo = string.Empty;
            do
            {
                nugetRepo = GetRepoPath(nugetRepo);
                if (!string.IsNullOrEmpty(nugetRepo))
                {
                    var nugetRepoFactory = PackageRepositoryFactory.Default.CreateRepository(nugetRepo);
                }
            } while (string.IsNullOrEmpty(nugetRepo));

            #endregion EnterPackageSource

            
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

        private static string GetRepoPath(string nugetRepo)
        {
            FontColour.ColourChangeDisplay("----------------------");
            Console.WriteLine("Enter a package source url");
            Console.WriteLine("----------------------");

            FontColour.ColourChangeResult();
            nugetRepo = Console.ReadLine();

            return Validation.UriExists(nugetRepo);
        }

        private static string GetFrameworkVersion(string targetFramework)
        {
            FontColour.ColourChangeDisplay("----------------------");
            Console.WriteLine("Enter a .NET Framework version number");
            Console.WriteLine("----------------------");

            FontColour.ColourChangeResult();
            targetFramework = Console.ReadLine();

            return Validation.RegexExists(targetFramework);
        }

        private static string GetSolutionPath(string repoFolder)
        {
            FontColour.ColourChangeDisplay("----------------------");
            Console.WriteLine("Enter a project solution path");
            Console.WriteLine("----------------------");

            FontColour.ColourChangeResult();
            repoFolder = Console.ReadLine();

            return Validation.DirectorExists(repoFolder);
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

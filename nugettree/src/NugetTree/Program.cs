
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
            //--------------------------------------------------------------------------------------------------------------
            #region EnterRepository

            var userInput = new UserInput();
            do
            {
                userInput.RepoFolder = GetSolutionPath(userInput.RepoFolder);

            } while (string.IsNullOrEmpty(userInput.RepoFolder));

            #endregion EnterRepository
            //--------------------------------------------------------------------------------------------------------------
            #region EnterFramework

            do
            {
                userInput.TargetFramework = GetFrameworkVersion(userInput.TargetFramework);
                if (!string.IsNullOrEmpty(userInput.TargetFramework))
                {
                    userInput.FrameworkName = new FrameworkName(userInput.TargetFramework);
                }

            } while (string.IsNullOrEmpty(userInput.TargetFramework));

            #endregion EnterFramework
            //--------------------------------------------------------------------------------------------------------------
            #region EnterPackageSource

            do
            {
                userInput.PackageSource = GetRepoPath(userInput.PackageSource);
                if (!string.IsNullOrEmpty(userInput.PackageSource))
                {
                    userInput.NugetRepoFactory = PackageRepositoryFactory.Default.CreateRepository(userInput.PackageSource);
                }

            } while (string.IsNullOrEmpty(userInput.PackageSource));

            #endregion EnterPackageSource
            //--------------------------------------------------------------------------------------------------------------
            #region Results

            userInput.Dependencies = new FindDependencies(userInput.RepoFolder, userInput.NugetRepoFactory).ListAll();

            foreach (var item in userInput.Dependencies)
            {
                Console.WriteLine();
                Console.WriteLine("--------------------------------------");
                Console.WriteLine(item.Project);

                OutputGraph(userInput.NugetRepoFactory, item.Packages, userInput.FrameworkName, item.LatestVersion, 0);
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();

            #endregion Results

        }

        private static string GetRepoPath(string packageSource)
        {
            FontColour.ColourChangeDisplay("----------------------");
            Console.WriteLine("Enter a package source URL. Otherwise ENTER to continue to use nuget.org");
            Console.WriteLine("----------------------");

            FontColour.ColourChangeResult();
            packageSource = Console.ReadLine();

            // If empty use default nuget.org package source
            if (string.IsNullOrEmpty(packageSource))
            {
                packageSource = @"https://api.nuget.org/v3/index.json";
            }

            return Validation.UriExists(packageSource);
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

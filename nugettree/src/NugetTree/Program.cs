
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
            var apiProperty = new ApiProperties();
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
                    apiProperty.FrameworkName = new FrameworkName(userInput.TargetFramework);
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
                    apiProperty.NugetRepoFactory = PackageRepositoryFactory.Default.CreateRepository(userInput.PackageSource);
                    apiProperty.Dependencies = new FindDependencies(userInput.RepoFolder, apiProperty.NugetRepoFactory).ListAll();
                }
                else
                {
                    // Use latest nuget source v3
                    apiProperty.NugetPackageSource = new NuGet.Configuration.PackageSource("https://api.nuget.org/v3/index.json");
                }

            } while (string.IsNullOrEmpty(userInput.PackageSource && apiProperty.NugetPackageSource != null));

            #endregion EnterPackageSource
            //--------------------------------------------------------------------------------------------------------------
            #region Results

            

            foreach (var item in apiProperty.Dependencies)
            {
                Console.WriteLine();
                Console.WriteLine("--------------------------------------");
                Console.WriteLine(item.Project);

                OutputGraph(apiProperty.NugetRepoFactory, item.Packages, apiProperty.FrameworkName, item.LatestVersion, 0);
            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();

            #endregion Results

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

        private static string GetRepoPath(string packageSource)
        {
            FontColour.ColourChangeDisplay("----------------------");
            Console.WriteLine("Enter a package source URL. Otherwise ENTER to continue to use nuget.org V3");
            Console.WriteLine("----------------------");

            FontColour.ColourChangeResult();
            packageSource = Console.ReadLine();

            packageSource = Validation.UriExists(packageSource);
            Console.WriteLine(packageSource);
            return packageSource;
        }

        private static string GetFrameworkVersion(string targetFramework)
        {
            FontColour.ColourChangeDisplay("----------------------");
            Console.WriteLine("Enter a .NET Framework version number");
            Console.WriteLine("----------------------");

            FontColour.ColourChangeResult();
            targetFramework = Console.ReadLine();

            targetFramework = Validation.RegexExists(targetFramework);
            Console.WriteLine(targetFramework);
            return targetFramework;
        }

        static void OutputGraph(IPackageRepository repository, IEnumerable<IPackage> packages, FrameworkName targetFramework, List<IPackage> latestVersion, int depth)
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

                    OutputGraph(repository, dependentPackages, targetFramework, latestVersion, depth + 3);
                }
            }
        }
    }
}

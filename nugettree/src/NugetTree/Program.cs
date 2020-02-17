
namespace NugetTree
{
    using NuGet;
    using NuGet.Protocol.Core.Types;
    using NugetTree.ApiResource;
    using NugetTree.Font;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Versioning;

    public class Program
    {
        public static UserInput _userInput;
        public static ApiProperties _apiProperties;

        static void Main()
        {
            _userInput = new UserInput();
            _apiProperties = new ApiProperties();

            //--------------------------------------------------------------------------------------------------------------
            #region EnterRepository

            do
            {
                _userInput.RepoFolder = GetSolutionPath(_userInput.RepoFolder);

            } while (string.IsNullOrEmpty(_userInput.RepoFolder));

            #endregion EnterRepository
            //--------------------------------------------------------------------------------------------------------------
            #region EnterFramework

            do
            {
                _userInput.TargetFramework = GetFrameworkVersion(_userInput.TargetFramework);
                if (!string.IsNullOrEmpty(_userInput.TargetFramework))
                {
                    _apiProperties.FrameworkName = new FrameworkName(_userInput.TargetFramework);
                }

            } while (string.IsNullOrEmpty(_userInput.TargetFramework));

            #endregion EnterFramework
            //--------------------------------------------------------------------------------------------------------------
            #region EnterPackageSource

            do
            {
                _userInput.PackageSource = GetRepoPath(_userInput.PackageSource);

                if (_userInput.UseLatest)
                {
                    // Use latest nuget source v3 // 
                    _apiProperties.NugetPackageSource = new NuGet.Configuration.PackageSource(_userInput.PackageSource);
                    _apiProperties.Dependencies = new NugetSearchResource(_userInput, _apiProperties).ListAll();
                }
                else
                {
                    // https://www.nuget.org/api/v2/
                    _apiProperties.NugetOldFactory = PackageRepositoryFactory.Default.CreateRepository(_userInput.PackageSource);
                    _apiProperties.Dependencies = new FindDependencies(_userInput.RepoFolder, _apiProperties.NugetOldFactory).ListAll();
                }

            } while (string.IsNullOrEmpty(_userInput.PackageSource));

            #endregion EnterPackageSource
            //--------------------------------------------------------------------------------------------------------------
            #region Results

            foreach (var item in _apiProperties.Dependencies)
            {
                Console.WriteLine();
                Console.WriteLine("--------------------------------------");
                Console.WriteLine(item.Project);

                if (_apiProperties.NugetPackageSource != null)
                {
                    NewOutputGraph(_apiProperties.NugetNewFactory, item.Packages, _apiProperties.FrameworkName, item.LatestVersion, 0);
                }
                else
                {
                    OutputGraph(_apiProperties.NugetOldFactory, item.Packages, _apiProperties.FrameworkName, item.LatestVersion, 0);
                }

            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();

            #endregion Results
            //--------------------------------------------------------------------------------------------------------------
        }

        private static string GetSolutionPath(string repoFolder)
        {
            FontColour.ColourChangeDisplay("-----------------------------------");
            Console.WriteLine("Enter a project solution path");
            Console.WriteLine("-----------------------------------");

            FontColour.ColourChangeResult();
            repoFolder = Console.ReadLine();

            return Validation.DirectorExists(repoFolder);
        }

        private static string GetRepoPath(string packageSource)
        {
            _userInput.UseLatest = false;
            FontColour.ColourChangeDisplay("-----------------------------------");
            Console.WriteLine("Enter a package source URL. Otherwise ENTER to continue to use nuget.org V3");
            Console.WriteLine("-----------------------------------");

            FontColour.ColourChangeResult();
            packageSource = Console.ReadLine();

            // If blank use default V3 package source
            if (string.IsNullOrEmpty(packageSource))
            {
                packageSource = "https://api.nuget.org/v3/index.json";
                _userInput.UseLatest = true;
                Console.WriteLine(packageSource);
            }
            else
            {
                packageSource = Validation.UriExists(packageSource);
                if (packageSource == "Error")
                {
                    packageSource = string.Empty;
                }
            }

            return packageSource;
        }

        private static string GetFrameworkVersion(string targetFramework)
        {
            FontColour.ColourChangeDisplay("-----------------------------------");
            Console.WriteLine("Enter a .NET Framework version number");
            Console.WriteLine("-----------------------------------");

            FontColour.ColourChangeResult();
            targetFramework = Console.ReadLine();

            targetFramework = Validation.RegexExists(targetFramework);
            Console.WriteLine($"{targetFramework} \r\n");
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
        static void NewOutputGraph(IPackageSearchMetadata nugetOldFactory, List<IPackage> packages, FrameworkName frameworkName, List<IPackage> latestVersion, int v)
        {
            throw new NotImplementedException();
        }
    }
}

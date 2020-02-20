
namespace NugetTree
{
    using NuGet;
    using NugetTree.ApiResource;
    using NugetTree.Font;
    using NugetTree.Output;
    using System;
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
                else if(_userInput.PackageSource != "Error")
                {
                    // https://www.nuget.org/api/v2/
                    _apiProperties.NugetOldFactory = PackageRepositoryFactory.Default.CreateRepository(_userInput.PackageSource);
                    _apiProperties.Dependencies = new FindDependencies(_userInput.RepoFolder, _apiProperties.NugetOldFactory).ListAll();
                }
                else
                {
                    _userInput.PackageSource = string.Empty;
                    Console.Clear();
                }

            } while (string.IsNullOrEmpty(_userInput.PackageSource));

            #endregion EnterPackageSource
            //--------------------------------------------------------------------------------------------------------------
            #region Results

            foreach (var item in _apiProperties.Dependencies)
            {
                FontColour.ColourChangeDisplay($"{Environment.NewLine}--------------------------------------");
                Console.WriteLine($"Project Folder - {item.Project}");
                Console.WriteLine("--------------------------------------");

                OutputGraph op = new OutputGraph();
                if (_apiProperties.NugetPackageSource != null)
                {
                    op.OutputV3Graph(item.LocalVersionMetaData, _apiProperties.FrameworkName, item.LatestVersionMetaData, 0);
                }
                else
                {
                    op.OutputV2Graph(_apiProperties.NugetOldFactory, item.Packages, _apiProperties.FrameworkName, item.LatestVersion, 0);
                }

            }

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
            Console.ReadLine();

            #endregion Results
            //--------------------------------------------------------------------------------------------------------------
        }

        //--------------------------------------------------------------------------------------------------------------
        private static string GetSolutionPath(string repoFolder)
        {
            FontColour.ColourChangeDisplay("-----------------------------------");
            Console.WriteLine("Enter a project solution path");
            Console.WriteLine("-----------------------------------");

            FontColour.ColourChangeResult();
            repoFolder = Console.ReadLine();

            return Validation.DirectorExists(repoFolder);
        }
        //--------------------------------------------------------------------------------------------------------------
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
            }

            return packageSource;
        }
        //--------------------------------------------------------------------------------------------------------------
        private static string GetFrameworkVersion(string targetFramework)
        {
            FontColour.ColourChangeDisplay("-----------------------------------");
            Console.WriteLine("Enter a .NET Framework version number");
            Console.WriteLine("-----------------------------------");

            FontColour.ColourChangeResult();
            targetFramework = Console.ReadLine();

            targetFramework = Validation.RegexExists(targetFramework);
            Console.WriteLine($"{targetFramework} \r\n");
            Console.ReadLine();
            Console.Clear();

            return targetFramework;
        }
        //--------------------------------------------------------------------------------------------------------------
    }
}

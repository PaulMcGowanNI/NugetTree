
namespace NugetTree
{
    using NugetTree.Font;
    using System;
    using System.IO;
    using System.Text.RegularExpressions;

    public static class Validation
    {
        public static string DirectorExists(string repoFolder)
        {
            if (!string.IsNullOrEmpty(repoFolder))
            {
                if (!Directory.Exists(repoFolder))
                {
                    //  Directory does not exist
                    FontColour.ColourChangeError($"This directory not found: \n {repoFolder} \n Please double check file path. \nPlease Double check that this program has read and write access to the directory.\n");
                    repoFolder = string.Empty;
                    Console.ReadKey();
                }
            }
            return repoFolder;
        }

        public static string RegexExists(string frameworkVersion)
        {
            if (!string.IsNullOrEmpty(frameworkVersion))
            {
                var regex = @"^[0-9]{1,11}(?:\.[0-9]{1,3}(\.[0-9]{1,3})?)$";
                var match = Regex.Match(frameworkVersion, regex);
                if (!match.Success)
                {
                    //  Does not match a .NET pattern
                    FontColour.ColourChangeError($"Not a valid .NET pattern - {frameworkVersion}");
                    frameworkVersion = string.Empty;
                }
                else
                {
                    // User in put framework version
                    frameworkVersion = $".NETFramework,Version=v{frameworkVersion}";
                }

                return frameworkVersion;
            }
            else
            {
                // Use the latest framework if none specified
                return frameworkVersion = $".NETFramework,Version=v4.7.2";
            }
        }

        public static string UriExists(string packageSourcePath)
        {
            if (!string.IsNullOrEmpty(packageSourcePath))
            {
                Uri uriResult;
                bool result = Uri.TryCreate(packageSourcePath, UriKind.Absolute, out uriResult)
                    && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
                if (!result)
                {
                    //  does not match a .NET pattern
                    FontColour.ColourChangeError($"Not a valid package source path - {uriResult}");
                    packageSourcePath = string.Empty;
                }
            }
            return packageSourcePath;
        }
    }
}

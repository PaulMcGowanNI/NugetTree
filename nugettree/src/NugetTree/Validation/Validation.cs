
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
            if (!Directory.Exists(repoFolder))
            {
                //  Directory does not exist
                FontColour.ColourChangeError($"This directory not found: \n {repoFolder} \n Please double check file path. \nPlease Double check that this program has read and write access to the directory.\n");
                repoFolder = string.Empty;
                Console.ReadKey();
            }
            return repoFolder;
        }

        public static string RegexExists(string frameworkVersion)
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
                frameworkVersion = $".NETFramework,Version=v{frameworkVersion}";
            }
            return frameworkVersion;
        }

        public static string UriExists(string packageSourcePath)
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

            return packageSourcePath;
        }
    }
}


namespace NugetTree.Font
{
    using NuGet.Versioning;
    using System;
    public static class FontColour
    {
        public static void ColourChangeDisplay(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
        }

        public static void ColourChangeError(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
        }

        public static void ColourChangeResult()
        {
            Console.ForegroundColor = ConsoleColor.Green;
        }

        public static NuGet.SemanticVersion NugetColor(NuGet.SemanticVersion version)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            return version;
        }

        public static NuGetVersion NuGetVersion(NuGetVersion version)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            return version;
        }

        public static void NormalColor()
        {
            Console.ForegroundColor = ConsoleColor.Gray;
        }
    }
}

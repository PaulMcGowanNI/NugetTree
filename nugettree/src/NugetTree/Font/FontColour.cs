
namespace NugetTree.Font
{
    using NuGet;
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

        public static SemanticVersion NugetColor(SemanticVersion version)
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

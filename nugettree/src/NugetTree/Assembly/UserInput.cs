
namespace NugetTree
{
    public class UserInput
    {
        public string RepoFolder { get; set; } = string.Empty;

        public string TargetFramework { get; set; } = string.Empty;

        public string PackageSource { get; set; } = string.Empty;

        public bool UseLatest { get; set; }
    }
}

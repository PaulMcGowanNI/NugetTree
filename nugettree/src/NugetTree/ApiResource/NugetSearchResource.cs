
namespace NugetTree.ApiResource
{
    using NuGet.Protocol.Core.Types;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class NugetSearchResource
    {
        private UserInput _userInput;
        private ApiProperties _apiProperties;
        private IPackageSearchMetadata _nugetSearchRepo;

        public NugetSearchResource(UserInput userInput, ApiProperties apiProperties, IPackageSearchMetadata nugetSearchRepo)
        {
            _userInput = userInput;
            _apiProperties = apiProperties;
        }

        public List<PackageSummaries> ListAll()
        {
            return SearchDependency(true);
        }

        private List<PackageSummaries> SearchDependency(bool showEmptyResults)
        {
            var package = new List<PackageSummaries>();
            var folders = Directory.EnumerateDirectories(_userInput.RepoFolder).Where(f => !f.StartsWith("."));

            if (!folders.Any(f => !f.EndsWith("\\bin") && !f.EndsWith("\\obj")))
            {
                folders = new[] { _userInput.RepoFolder };
            }

            return package;
        }
    }
}

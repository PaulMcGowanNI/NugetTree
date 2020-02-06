
namespace NugetTree.ApiResource
{
    using NuGet.Protocol.Core.Types;
    using System.Collections.Generic;

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

        public void NewPackagePath()
        {
            // Use latest nuget source v3
            _apiProperties.NugetPackageSource = new NuGet.Configuration.PackageSource("https://api.nuget.org/v3/index.json");
        }

        public List<PackageSummaries> ListAll()
        {
            return Search(true);
        }

        private List<PackageSummaries> Search(bool showEmptyResults)
        {
            return new List<PackageSummaries>();
        }
    }
}

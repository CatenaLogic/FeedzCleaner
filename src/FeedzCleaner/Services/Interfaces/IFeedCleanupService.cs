namespace FeedzCleaner.Services
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IFeedCleanupService
    {
        Task AutomaticallySelectRemovablePackagesAsync(List<Package> packages);
    }
}

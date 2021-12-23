namespace FeedzCleaner.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IFeedService
    {
        Task DeletePackagesAsync(CleanupFeedsContext context, Action<Package, int, int> progress);
        Task<List<Package>> IndexPackagesAsync(FeedsContext context);
    }
}

namespace FeedzCleaner.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Catel.Logging;
    using Feedz.Client;
    using MethodTimer;

    public class FeedService : IFeedService
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        [Time]
        public async Task<List<Package>> IndexPackagesAsync(FeedsContext context)
        {
            var packages = new List<Package>();

            using (var client = FeedzClient.Create(context.AccessToken))
            {
                var repositoryScope = client.ScopeToRepository(context.OrganizationName, context.Repository.Slug);

                var feedzPackages = await repositoryScope.Packages.List();

                packages.AddRange(from x in feedzPackages
                                  orderby x.Id
                                  orderby x.Version descending
                                  select new Package(x));
            }

            return packages;
        }

        [Time]
        public async Task DeletePackagesAsync(CleanupFeedsContext context, Action<Package, int, int> progress)
        {
            using (var client = FeedzClient.Create(context.AccessToken))
            {
                var repositoryScope = client.ScopeToRepository(context.OrganizationName, context.Repository.Slug);

                Log.Info($"Cleaning up feed '{repositoryScope.RootUri}'");

                var packages = await repositoryScope.Packages.List();

                var distinctPackages = new HashSet<string>(packages.Select(x => x.PackageId), StringComparer.OrdinalIgnoreCase);

                foreach (var package in packages)
                {
                    if (!context.PackagesToRemove.Any(x => x.Id == package.Id))
                    {
                        Log.Debug($"Keeping package {package.Id} v{package.Version}");
                        continue;
                    }

                    Log.Debug($"Removing package {package.Id} v{package.Version}");

                    if (!context.IsDryRun)
                    {
                        await repositoryScope.PackageFeed.Remove(package);
                    }
                }
            }
        }
    }
}

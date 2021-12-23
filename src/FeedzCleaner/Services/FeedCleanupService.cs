namespace FeedzCleaner.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Catel.Logging;
    using MethodTimer;

    public class FeedCleanupService : IFeedCleanupService
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public FeedCleanupService()
        {
            
        }

        [Time]
        public async Task AutomaticallySelectRemovablePackagesAsync(List<Package> packages)
        {
            var distinctPackages = new HashSet<string>(packages.Select(x => x.PackageId), StringComparer.OrdinalIgnoreCase);

            foreach (var distinctPackage in distinctPackages)
            {
                var packageVersions = (from package in packages
                                       where package.PackageId == distinctPackage
                                       select package).ToList();

                await AutomaticallySelectRemovablePackagesAsync(distinctPackage, packageVersions);
            }
        }

        protected async Task AutomaticallySelectRemovablePackagesAsync(string packageId, List<Package> packages)
        {
            Log.Info($"  Automatically selecting packages to be removed for '{packageId}'");

            var descSsortedVersions = packages.Select(x => new Tuple<Package, SemanticVersioning.Version>(x, new SemanticVersioning.Version(x.Version))).OrderByDescending(x => x.Item2).ToList();
            var ascSortedVersions = descSsortedVersions.OrderBy(x => x.Item2).ToList();
            var lastStableVersion = descSsortedVersions.FirstOrDefault(x => !x.Item2.IsPreRelease)?.Item2;
            if (lastStableVersion is null)
            {
                Log.Info($"    No stable versions available, keeping all versions");
                return;
            }

            Log.Info($"    Last stable version: {lastStableVersion}");

            foreach (var packageWithVersion in ascSortedVersions)
            {
                var package = packageWithVersion.Item1;
                var version = packageWithVersion.Item2;

                package.ToBeRemoved = false;

                if (!version.IsPreRelease)
                {
                    Log.Debug($"    Keeping stable package version '{version}'");
                    continue;
                }

                if (version > lastStableVersion)
                {
                    Log.Debug($"    Keeping prerelease package version '{version}'");
                    continue;
                }

                //if (version.PreRelease.Contains("beta", StringComparison.Ordinal))
                //{
                //    if (version.BaseVersion() == lastStableVersion)
                //    {
                //        Log.Debug($"    Keeping beta package version '{version}'");
                //        continue;
                //    }
                //}

                Log.Info($"    Marking prerelease package version '{version}' to be removed");

                package.ToBeRemoved = true;
            }
        }
    }
}

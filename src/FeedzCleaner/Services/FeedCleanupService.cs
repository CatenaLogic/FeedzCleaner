namespace FeedzCleaner.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using MethodTimer;
    using Microsoft.Extensions.Logging;

    public class FeedCleanupService : IFeedCleanupService
    {
        private readonly ILogger<FeedCleanupService> _logger;

        public FeedCleanupService(ILogger<FeedCleanupService> logger)
        {
            ArgumentNullException.ThrowIfNull(logger);

            _logger = logger;
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
            _logger.LogInformation("  Automatically selecting packages to be removed for '{PackageId}'", packageId);

            var descSsortedVersions = packages.Select(x => new Tuple<Package, SemanticVersioning.Version>(x, new SemanticVersioning.Version(x.Version))).OrderByDescending(x => x.Item2).ToList();
            var ascSortedVersions = descSsortedVersions.OrderBy(x => x.Item2).ToList();
            var lastStableVersion = descSsortedVersions.FirstOrDefault(x => !x.Item2.IsPreRelease)?.Item2;
            if (lastStableVersion is null)
            {
                _logger.LogInformation("    No stable versions available, keeping all versions");
                return;
            }

            _logger.LogInformation("    Last stable version: {Version}", lastStableVersion);

            foreach (var packageWithVersion in ascSortedVersions)
            {
                var package = packageWithVersion.Item1;
                var version = packageWithVersion.Item2;

                package.ToBeRemoved = false;

                if (!version.IsPreRelease)
                {
                    _logger.LogDebug("    Keeping stable package version '{Version}'", version);
                    continue;
                }

                if (version > lastStableVersion)
                {
                    _logger.LogDebug("    Keeping prerelease package version '{Version}'", version);
                    continue;
                }

                //if (version.PreRelease.Contains("beta", StringComparison.Ordinal))
                //{
                //    if (version.BaseVersion() == lastStableVersion)
                //    {
                //    _logger.LogDebug("Keeping beta package version '{Version}'", version);
                //        continue;
                //    }
                //}

                _logger.LogInformation("    Marking prerelease package version '{Version}' to be removed", version);

                package.ToBeRemoved = true;
            }
        }
    }
}

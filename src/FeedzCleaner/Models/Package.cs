namespace FeedzCleaner
{
    using System;
    using Feedz.Client.Resources;

    public class Package
    {
        public Package(PackageResource packageResource)
        {
            Id = packageResource.Id;
            PackageId = packageResource.PackageId;
            Version = packageResource.Version;
            PackageSize = packageResource.PackageSize;
        }

        public Package(FeedPackageResult feedPackageResult)
        {
            Id = feedPackageResult.Id;
            PackageId = feedPackageResult.PackageId;
            Version = feedPackageResult.Version;
            PackageSize = feedPackageResult.PackageSize;
        }

        public Guid Id { get; set; }

        public string PackageId { get; set; }

        public string Version { get; set; }

        public long PackageSize { get; set; }

        public bool ToBeRemoved { get; set; }
    }
}

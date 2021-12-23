namespace FeedzCleaner
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class CleanupFeedsContext : FeedsContext
    {
        public CleanupFeedsContext(string name)
        {
            Name = name;
            PackagesToRemove = new List<Package>();
        }

        public bool IsDryRun { get; set; }

        public List<Package> PackagesToRemove { get; private set; }
    }
}

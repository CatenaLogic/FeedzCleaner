namespace FeedzCleaner
{
    using System.Collections.Generic;

    public class RepositoryContext
    {
        public RepositoryContext()
        {
            Name = string.Empty;
            Slug = string.Empty;
        }

        public string Name { get; set; }

        public string Slug { get; set; }
    }
}

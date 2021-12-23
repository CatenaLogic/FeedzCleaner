namespace FeedzCleaner
{
    public class FeedsContext
    {
        public FeedsContext()
        {
            Repository = new RepositoryContext();

            AccessToken = string.Empty;
            Name = string.Empty;
            OrganizationName = string.Empty;
        }

        public string AccessToken { get; set; }

        public string Name { get; set; }

        public string OrganizationName { get; set; }

        public RepositoryContext Repository { get; private set; }
    }
}

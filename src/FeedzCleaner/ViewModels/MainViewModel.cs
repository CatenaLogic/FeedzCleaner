namespace FeedzCleaner.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Catel;
    using Catel.Configuration;
    using Catel.Logging;
    using Catel.MVVM;
    using Catel.Reflection;
    using Catel.Services;
    using Humanizer;
    using MethodTimer;
    using Services;

    internal class MainViewModel : ViewModelBase
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly IFeedService _feedService;
        private readonly IFeedCleanupService _feedCleanupService;
        private readonly IDispatcherService _dispatcherService;
        private readonly IConfigurationService _configurationService;
        private readonly IMessageService _messageService;

        public MainViewModel(IFeedService feedService, IFeedCleanupService feedCleanupService,
            IDispatcherService dispatcherService, IConfigurationService configurationService,
            IMessageService messageService)
        {
            ArgumentNullException.ThrowIfNull(feedService);
            ArgumentNullException.ThrowIfNull(feedCleanupService);
            ArgumentNullException.ThrowIfNull(dispatcherService);
            ArgumentNullException.ThrowIfNull(configurationService);
            ArgumentNullException.ThrowIfNull(messageService);

            _feedService = feedService;
            _feedCleanupService = feedCleanupService;
            _dispatcherService = dispatcherService;
            _configurationService = configurationService;
            _messageService = messageService;

            Analyze = new Command(OnAnalyzeExecute, OnAnalyzeCanExecute);
            FakeCleanUp = new Command(OnFakeCleanUpExecute, OnCleanUpCanExecute);
            CleanUp = new Command(OnCleanUpExecute, OnCleanUpCanExecute);

            var entryAssembly = AssemblyHelper.GetEntryAssembly();
            Title = string.Format("{0} - v{1}", entryAssembly.Title(), entryAssembly.InformationalVersion());
        }

        public string Organization { get; set; }

        public string FeedName { get; set; }

        public string ApiToken { get; set; }

        public List<Package> Packages { get; private set; }

        public string TotalSizeSaved { get; private set; }

        public long TotalPackagesRemoved { get; private set; }

        public long TotalPackagesKept { get; private set; }

        public bool IsBusy { get; private set; }

        public int Progress { get; private set; }

        #region Commands
        public Command Analyze { get; private set; }

        private bool OnAnalyzeCanExecute()
        {
            if (string.IsNullOrWhiteSpace(Organization))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(FeedName))
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(ApiToken))
            {
                return false;
            }

            if (IsBusy)
            {
                return false;
            }

            return true;
        }

        private async void OnAnalyzeExecute()
        {
            try
            {
                await FindPackagesAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to find packages");
            }
        }

        public Command FakeCleanUp { get; private set; }

        private async void OnFakeCleanUpExecute()
        {
            await CleanAsync(true);
        }

        public Command CleanUp { get; private set; }

        private bool OnCleanUpCanExecute()
        {
            var packages = Packages;
            if (packages is null || packages.Count == 0)
            {
                return false;
            }

            if (IsBusy)
            {
                return false;
            }

            return true;
        }

        private async void OnCleanUpExecute()
        {
            await CleanAsync(false);
        }
        #endregion

        #region Methods
        protected override async Task InitializeAsync()
        {
            Organization = _configurationService.GetRoamingValue<string>(Settings.Application.General.LastOrganization);
            FeedName = _configurationService.GetRoamingValue<string>(Settings.Application.General.LastFeedName);

            await FindPackagesAsync();
        }

        public void UpdateSizeSaved()
        {
            var value = "-";
            var removed = 0;
            var kept = 0;

            var packages = Packages;
            if (packages is not null)
            {
                var totalBytes = (from package in packages
                                  where package.ToBeRemoved
                                  select package.PackageSize).Sum();

                value = totalBytes.Bytes().Humanize();

                removed = packages.Count(x => x.ToBeRemoved);
                kept = packages.Count - removed;
            }

            TotalSizeSaved = value;
            TotalPackagesRemoved = removed;
            TotalPackagesKept = kept;
        }

        [Time]
        private async Task FindPackagesAsync()
        {
            var organization = Organization;
            if (string.IsNullOrWhiteSpace(organization))
            {
                return;
            }

            var feedName = FeedName;
            if (string.IsNullOrWhiteSpace(feedName))
            {
                return;
            }

            var apiToken = ApiToken;
            if (string.IsNullOrWhiteSpace(apiToken))
            {
                return;
            }

            Log.Info("Start indexing packages");

            Progress = 0;

            using (CreateIsBusyScope())
            {
                var context = new FeedsContext
                {
                    OrganizationName = organization,
                    Name = feedName,
                    AccessToken = apiToken
                };

                context.Repository.Name = feedName;
                context.Repository.Slug = feedName;

                var packages = await _feedService.IndexPackagesAsync(context);

                Progress = 50;

                await _feedCleanupService.AutomaticallySelectRemovablePackagesAsync(packages);

                Progress = 75;

                Packages = (packages.Count > 0) ? packages : null;
                UpdateSizeSaved();

                _configurationService.SetRoamingValue(Settings.Application.General.LastOrganization, organization);
                _configurationService.SetRoamingValue(Settings.Application.General.LastFeedName, feedName);
            }

            Log.Info("Finished indexing packages");

            Progress = 100;
        }

        private async Task CleanAsync(bool isFakeClean)
        {
            var organization = Organization;
            if (string.IsNullOrWhiteSpace(organization))
            {
                return;
            }

            var feedName = FeedName;
            if (string.IsNullOrWhiteSpace(feedName))
            {
                return;
            }

            var apiToken = ApiToken;
            if (string.IsNullOrWhiteSpace(apiToken))
            {
                return;
            }

            Progress = 0;

            using (CreateIsBusyScope())
            {
                var context = new CleanupFeedsContext(feedName)
                {
                    OrganizationName = organization,
                    AccessToken = apiToken,
                    IsDryRun = isFakeClean,
                };

                context.Repository.Name = feedName;
                context.Repository.Slug = feedName;

                context.PackagesToRemove.AddRange(from package in Packages
                                                  where package.ToBeRemoved
                                                  select package);

                await _feedService.DeletePackagesAsync(context,
                    (package, currentItem, totalItems) => _dispatcherService.BeginInvoke(() =>
                    {
                        var percentage = ((double)currentItem / totalItems) * 100;
                        Progress = (int)percentage;
                    }));

                ViewModelCommandManager.InvalidateCommands(true);
            }

            Progress = 100;

            if (!isFakeClean)
            {
                Analyze.Execute(null);
            }
        }

        private IDisposable CreateIsBusyScope()
        {
            return new DisposableToken<MainViewModel>(this, x =>
            {
                x.Instance._dispatcherService.BeginInvoke(() => x.Instance.IsBusy = true);
            }, x =>
            {
                x.Instance._dispatcherService.BeginInvoke(() => x.Instance.IsBusy = false);
            });
        }
        #endregion
    }
}

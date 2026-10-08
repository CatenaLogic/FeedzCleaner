namespace FeedzCleaner
{
    using System;
    using System.Windows;
    using System.Windows.Media;
    using Catel;
    using Catel.Configuration;
    using Catel.IoC;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Orc;
    using Orchestra;
    using FeedzCleaner.Services;
    using Orc.Theming;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
#pragma warning disable IDISP006 // Implement IDisposable
        private readonly IHost _host;
#pragma warning restore IDISP006 // Implement IDisposable

        public App()
        {
            var hostBuilder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddLogging(logging =>
                    {
                        logging.SetMinimumLevel(LogLevel.Debug);
                        logging.AddDebug();
                        logging.AddInMemory();
                    });

                    services.AddCatelCore();
                    services.AddCatelMvvm();

                    services.AddOrcControls();
                    services.AddOrcFileSystem();
                    services.AddOrcLogViewer();
                    services.AddOrcNotifications();
                    services.AddOrcSerializationJson();
                    services.AddOrcSystemInfo();
                    services.AddOrcTheming();

                    services.AddOrchestraCore();

                    services.AddSingleton<IFeedService, FeedService>();
                    services.AddSingleton<IFeedCleanupService, FeedCleanupService>();
                });

            _host = hostBuilder.Build();

            IoCContainer.ServiceProvider = _host.Services;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Application.Startup"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.StartupEventArgs"/> that contains the event data.</param>
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var serviceProvider = IoCContainer.ServiceProvider;
            var configurationService = serviceProvider.GetRequiredService<IConfigurationService>();
            await configurationService.LoadAsync();

            serviceProvider.CreateTypesThatMustBeConstructedAtStartup();

            FontImage.RegisterFont("FontAwesome", new FontFamily(new Uri("pack://application:,,,/FeedzCleaner;component/Resources/Fonts/", UriKind.RelativeOrAbsolute), "./#FontAwesome"));
            FontImage.DefaultFontFamily = "FontAwesome";

            // This shows the StyleHelper, but uses a *copy* of the Orchestra themes. The default margins for controls are not defined in
            // Orc.Theming since it's a low-level library. The final default styles should be in the shell (thus Orchestra makes sense)
            StyleHelper.CreateStyleForwardersForDefaultStyles();
            ThemeManager.Current.SynchronizeTheme();

            var mainWindow = ActivatorUtilities.CreateInstance<Views.MainWindow>(_host.Services);
            mainWindow.Show();
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            using (_host)
            {
                await _host.StopAsync();
            }

            base.OnExit(e);
        }
    }
}

namespace FeedzCleaner.Tests;

using Catel;
using Microsoft.Extensions.DependencyInjection;
using Orc;
using Orchestra;

internal static class ServiceCollectionHelper
{
    public static IServiceCollection CreateServiceCollection()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddLogging();
        serviceCollection.AddCatelCore();
        serviceCollection.AddCatelMvvm();
        serviceCollection.AddOrcControls();
        serviceCollection.AddOrcLogViewer();
        serviceCollection.AddOrcTheming();
        serviceCollection.AddOrchestraCore();

        return serviceCollection;
    }
}

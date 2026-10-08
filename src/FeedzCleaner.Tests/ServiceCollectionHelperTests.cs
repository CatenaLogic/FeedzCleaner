namespace FeedzCleaner.Tests;

using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

[TestFixture]
internal sealed class ServiceCollectionHelperTests
{
    [Test]
    public void CreateServiceCollection_ReturnsIndependentCollections()
    {
        var firstServices = ServiceCollectionHelper.CreateServiceCollection();
        var secondServices = ServiceCollectionHelper.CreateServiceCollection();
        firstServices.AddSingleton<TestService>();

        using var firstProvider = firstServices.BuildServiceProvider();
        using var secondProvider = secondServices.BuildServiceProvider();

        Assert.That(firstProvider.GetService<TestService>(), Is.Not.Null);
        Assert.That(secondProvider.GetService<TestService>(), Is.Null);
    }

    private sealed class TestService
    {
    }
}

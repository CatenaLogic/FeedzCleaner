﻿using Catel.IoC;
using FeedzCleaner.Services;

/// <summary>
/// Used by the ModuleInit. All code inside the Initialize method is ran as soon as the assembly is loaded.
/// </summary>
public static class ModuleInitializer
{
    /// <summary>
    /// Initializes the module.
    /// </summary>
    public static void Initialize()
    {
        var serviceLocator = ServiceLocator.Default;

        serviceLocator.RegisterType<IFeedService, FeedService>();
        serviceLocator.RegisterType<IFeedCleanupService, FeedCleanupService>();
    }
}

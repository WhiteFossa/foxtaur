﻿using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.IO;
using Foxtaur.LibRenderer.Services.Abstractions;
using Foxtaur.LibRenderer.Services.Implementations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using NLog.Extensions.Logging;

namespace Foxtaur.Desktop
{
    class Program
    {
        /// <summary>
        /// Dependency injection service provider
        /// </summary>
        public static ServiceProvider Di {get; set; }
        
        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        [STAThread]
        public static void Main(string[] args)
        {
            // Preparing DI
            Di = ConfigureServices()
                .BuildServiceProvider();
            
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json",true, true);

            var configuration = builder.Build();
            
            // Setting-up NLog
            LogManager.Configuration = new NLogLoggingConfiguration(configuration.GetSection("NLog"));
            
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
        {
            return AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
        }
        
        // Setting up DI
        public static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            services.AddSingleton<ITexturesLoader, TexturesLoader>();

            return services;
        }
    }
}
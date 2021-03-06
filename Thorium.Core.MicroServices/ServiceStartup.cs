﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using SimpleInjector;
using Thorium.Core.MicroServices.Abstractions;
using Thorium.Core.MicroServices.ConfigurationModels;
using Thorium.Core.MicroServices.Endpoints;
using Thorium.Core.MicroServices.LivenessChecks;
using Thorium.Core.MicroServices.Redis;

namespace Thorium.Core.MicroServices
{
    public class ServiceStartup
    {
        private readonly IServiceStartup _startup;

        public ServiceStartup(IServiceStartup startup)
        {
            _startup = startup;
        }

        private static Container _container;
        private static IConfiguration _configuration;
        private CustomLoggerConfiguration _consoleLogging;

        private List<IServiceEndpoint> _handlers;

        private static ILogger _logger;
        private static List<DbSettings> _dbSettings;
        private ReadyCheckEndpoint _readyCheck;
        private LivenessCheckConfiguration _liveCheckConfig;
        private LiveCheckEndpoint _liveCheck;

        public void Initialize()
        {
            var appInfo = Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application;
            Console.WriteLine($"Starting {appInfo.ApplicationName} {appInfo.ApplicationVersion} ...");
            Console.WriteLine($"Framework: {appInfo.RuntimeFramework.FullName}");
            LoadSettings();
            ConfigureLogger();

            if (_startup is ISeedable seedable)
            {
                seedable?.SeedData(_dbSettings, _logger);
            }

            LoadEndPoints();
            _logger.Information($"Console Logging Level: {_consoleLogging?.ConsoleLoggingLevel ?? "Information"}");
        }

        private void LoadEndPoints()
        {
            InitializeContainer();

            _handlers = _container.GetAllInstances<IServiceEndpoint>().ToList();


            _readyCheck = new ReadyCheckEndpoint();
            _readyCheck.StartServer();

            _liveCheck = new LiveCheckEndpoint(_liveCheckConfig);
            _liveCheck.StartServer();

            var endpointsSucceeded = _handlers.ToList().All(TryServiceStart);
            _readyCheck.ServiceReadyStatus = endpointsSucceeded;
        }

        private bool TryServiceStart(IServiceEndpoint c)
        {
            _logger.Information("Starting service endpoint {endPointName}", c.EndpointDescription);
            try
            {
                c.StartServer();
                return true; // we assume synchronous starting, but maybe future readiness checks shouldn't
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "Unable to start service {endPointName}", c.EndpointDescription);
                throw ex;
            }
        }

        internal void StopServices()
        {
            _readyCheck.ServiceReadyStatus = false;
            _readyCheck.StopServer();
            _logger.Information("Stopping service...");
            foreach (var handler in _handlers)
            {
                _logger.Information("Stopping {endpointName}", handler.EndpointDescription);
                handler.StopServer();
            }

            _logger.Information("Exiting...");
        }

        public Container InitializeContainer()
        {
            _container = new Container();

            ConfigureContainer(_container);
            _container.Verify();
            return _container;
        }

        public void ConfigureContainer(Container container)
        {
            if (_dbSettings != null)
            {
                container.Register(() => _dbSettings);
            }

            container.Register(() => _configuration);

            container.Register(() => _logger);

            _liveCheckConfig = new LivenessCheckConfiguration(container);
            if (_startup is ICanCheckLiveness liveCheckSetup)
            {
                liveCheckSetup?.ConfigureLivenessChecks(_liveCheckConfig);
            }

            _liveCheckConfig.Build();
            container.Register<ILiveChecker>(() => _liveCheckConfig);

            var svcConfiguration = new ServiceConfiguration(container, _configuration);


            _startup.ConfigureService(svcConfiguration);
        }

        private string AppSettingsLocation => $"config{Path.DirectorySeparatorChar}appsettings.json";

        private RedisConfig LoadRedisSettings()
        {
            Console.WriteLine($"Checking whether Redis config should loaded/downloaded");
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(AppSettingsLocation)
                .Build()
                .GetSection(nameof(RedisConfig)).Get<RedisConfig>();
        }

        public IConfiguration LoadSettings()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory());

            var redis = LoadRedisSettings();
            if (redis != null)
            {
                builder.AddRedisConfig(new CancellationToken(), options =>
                {
                    options.RedisConfig = redis;
                    options.Path = AppSettingsLocation;
                    options.Optional = true;
                    options.ReloadOnChange = true;
                    options.OnReload = () =>
                    {
                        if (!(_readyCheck?.Ready ?? false))
                        {
                            return;
                        }

                        Console.WriteLine($"Settings have been updated...");
                        _logger?.Information($"Settings have been updated...");
                        StopServices();

                        Environment.Exit(0);
                    };
                });
            }

            builder.AddJsonFile(AppSettingsLocation, optional: false, reloadOnChange: false);

            _configuration = builder.Build();

            _dbSettings = _configuration.GetSection("DbSettings").Get<List<DbSettings>>();
            _consoleLogging = _configuration.GetSection("ConsoleLogging").Get<CustomLoggerConfiguration>();

            return _configuration;
        }

        public static List<DbSettings> DbSettings => _dbSettings;

        public void ConfigureLogger()
        {
            var config = new LoggerConfiguration()
                .ReadFrom.Configuration(_configuration)
                .Enrich.WithProperty("MicroService", Assembly.GetEntryAssembly().GetName().Name);

            var configPresent = _configuration.GetSection("Serilog").GetChildren().Any();

            if (!configPresent)
            {
                config.WriteTo.Console(LogEventLevel.Information);
            }

            if (_startup.GetType().IsAssignableFrom(typeof(ICustomLogging)))
            {
                var loggingConfig = _startup as ICustomLogging;
                loggingConfig?.ConfigureLogging(config);
            }

            _logger = config.CreateLogger();
            if (!configPresent)
            {
                _logger.Warning("This service has no logger configuration. Logging will only go to console.");
            }
        }

        public static Container GetContainer()
        {
            return _container;
        }

        public static IConfiguration GetConfigurationRoot()
        {
            return _configuration;
        }

        public static void ContainerSetup(Container container, IServiceStartup startup)
        {
            var serviceStartup = new ServiceStartup(startup);

            if (_configuration == null)
            {
                serviceStartup.LoadSettings();
                serviceStartup.ConfigureLogger();
            }

            serviceStartup.ConfigureContainer(container);
        }
    }
}
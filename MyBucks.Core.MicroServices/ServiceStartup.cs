using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.Configuration;
using MyBucks.Core.MicroServices.Abstractions;
using MyBucks.Core.MicroServices.ConfigurationModels;
using MyBucks.Core.MicroServices.Redis;
using Serilog;
using SimpleInjector;

namespace MyBucks.Core.MicroServices
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
            _handlers.ToList().ForEach(TryServiceStart);
        }

        private void TryServiceStart(IServiceEndpoint c)
        {
            _logger.Information("Starting service endpoint {endPointName}", c.EndpointDescription);
            try
            {
                c.StartServer();
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "Unable to start service {endPointName}", c.EndpointDescription);
                throw ex;
            }
        }

        internal void StopServices()
        {
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

            _startup.ConfigureService(new ServiceConfiguration(container, _configuration));
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
                        Console.WriteLine($"Settings have been updated...");
                        _logger?.Information($"Settings have been updated...");
                        StopServices();

                        Environment.Exit(0);
                    };
                });
            }
            else
                builder.AddJsonFile(AppSettingsLocation, optional: false, reloadOnChange: true);

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

            if (_startup.GetType().IsAssignableFrom(typeof(ICustomLogging)))
            {
                var loggingConfig = _startup as ICustomLogging;
                loggingConfig?.ConfigureLogging(config);
            }

            _logger = config.CreateLogger();
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
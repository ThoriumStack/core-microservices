using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using MyBucks.Core.MicroServices.Abstractions;
using MyBucks.Core.MicroServices.ConfigurationModels;
using Serilog;
using Serilog.Events;
using SimpleInjector;

namespace MyBucks.Core.MicroServices
{
    public class ServiceStartup
    {
        private IServiceStartup _startup;

        public ServiceStartup(IServiceStartup startup)
        {
            _startup = startup;
        }

        private static Container _container;
        private IConfigurationRoot _configuration;
        private CustomLoggerConfiguration _consoleLogging;

        private List<IServiceEndpoint> _handlers;

        private ILogger _logger;
        private DbSettings _dbSettings;

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

            _logger.Information($"Console Logging Level: {_consoleLogging.ConsoleLoggingLevel}");
        }

        private void LoadEndPoints()
        {
            InitializeContainer();

            _startup.LoadEndpoints(_container);

            _container.Verify();

            _handlers = _container.GetAllInstances<IServiceEndpoint>().ToList();
            _handlers.ToList().ForEach(c => c.StartServer());
        }

        private void InitializeContainer()
        {
            _container = new Container();
            _startup.RegisterStaticInstances(_container);

            if (_dbSettings != null)
            {
                _container.Register(() => _dbSettings);
            }

            _container.Register(() => _logger);

            _startup.RegisterServices(_container);
        }

        internal void StopServices()
        {
            foreach (var handler in _handlers)
            {
                handler.StopServer();
            }
        }

        public IConfigurationRoot LoadSettings()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config" + Path.DirectorySeparatorChar + "appsettings.json");

            _configuration = builder.Build();

            _dbSettings = new DbSettings
            {
                ConnectionString = _configuration.GetSection("ConnectionStrings")["DefaultConnection"]
            };

            _consoleLogging = _configuration.GetSection("ConsoleLogging").Get<CustomLoggerConfiguration>();
            _startup.LoadConfiguration(_configuration);
            return _configuration;
        }

        public void ConfigureLogger()
        {
            var level = Serilog.Events.LogEventLevel.Information;

            if ("Verbose".Equals(_consoleLogging.ConsoleLoggingLevel, StringComparison.InvariantCultureIgnoreCase))
            {
                level = Serilog.Events.LogEventLevel.Verbose;
            }

            _logger = new LoggerConfiguration()
                .WriteTo.Elasticsearch()
                .ReadFrom.Configuration(_configuration)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .WriteTo.Console(level)
                .CreateLogger();

            if (_startup.GetType().IsAssignableFrom(typeof(ICustomLogging)))
            {
                var loggingConfig = _startup as ICustomLogging;
                loggingConfig?.ConfigureLogging(_logger);
            }
        }

        public static Container GetContainer()
        {
            return _container;
        }
    }

    public interface ICustomLogging
    {
        void ConfigureLogging(ILogger logger);
    }
}
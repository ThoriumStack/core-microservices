﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private static IConfigurationRoot _configuration;
        private CustomLoggerConfiguration _consoleLogging;

        private List<IServiceEndpoint> _handlers;

        private static ILogger _logger;
        private static DbSettings _dbSettings;

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

            container.Register(() => _logger);

            _startup.ConfigureService(new ServiceConfiguration(container, _configuration));
            
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
            return _configuration;
        }

        public void ConfigureLogger()
        {
            var level = Serilog.Events.LogEventLevel.Information;
            if (_consoleLogging != null)
            {
                if ("Verbose".Equals(_consoleLogging.ConsoleLoggingLevel, StringComparison.InvariantCultureIgnoreCase))
                {
                    level = Serilog.Events.LogEventLevel.Verbose;
                }
            }


            var config = new LoggerConfiguration()
                .ReadFrom.Configuration(_configuration)
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .WriteTo.Console(level);
            
            if (!Debugger.IsAttached)
            {
                config.WriteTo.Elasticsearch();
            }
            
            _logger = config.CreateLogger();

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
        
        public static IConfigurationRoot GetConfigurationRoot()
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
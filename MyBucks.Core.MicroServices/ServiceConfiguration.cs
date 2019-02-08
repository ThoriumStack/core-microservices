using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using MyBucks.Core.MicroServices.Abstractions;
using MyBucks.Core.MicroServices.Mappers;
using MyBucks.Core.Model.Abstractions;
using SimpleInjector;

namespace MyBucks.Core.MicroServices
{
    public class ServiceConfiguration
    {
        private readonly Container _container;
        private readonly IConfiguration _configRoot;

        internal ServiceConfiguration(Container container, IConfiguration configRoot)
        {
            _container = container;
            _configRoot = configRoot;
        }
        
        public void AddServiceEndpoint<TEndpoint>() where TEndpoint : class, IServiceEndpoint
        {
            _container.Collection.Append<IServiceEndpoint, TEndpoint>();            
        }

        
        /// <summary>
        /// Add a configuration element. The typename must match with the key in appsettings.json
        /// </summary>
        /// <typeparam name="TConfiguration"></typeparam>
        public TConfiguration AddConfiguration<TConfiguration>() where TConfiguration : class
        {
            var oType = typeof(TConfiguration);
            var isList =(oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeof(List<>)));
            
            var typeName = typeof(TConfiguration).Name;
            if (isList)
            {
                typeName = oType.GenericTypeArguments.First().Name;
            }

            var configItem = _configRoot.GetSection(typeName).Get<TConfiguration>();
            
            _container.Register(() => configItem);
            return configItem;
        }
        
        /// <summary>
        /// Get a configuration object based on a section key.
        /// </summary>
        /// <param name="sectionKey"></param>
        /// <typeparam name="TConfiguration"></typeparam>
        public void AddConfiguration<TConfiguration>(string sectionKey) where TConfiguration : class
        {
            var configItem = _configRoot.GetSection(sectionKey).Get<TConfiguration>();
            _container.Register(() => configItem);
        }
        
        /// <summary>
        /// Add a custom configuration. Get access to the .net Configuration Root.
        /// </summary>
        /// <param name="configure"></param>
        /// <typeparam name="TConfiguration"></typeparam>
        public TConfiguration AddConfiguration<TConfiguration>(Func<IConfiguration, TConfiguration> configure) where TConfiguration : class
        {
            var config = configure(_configRoot);
            _container.Register(() => config);
            return config;
        }

        public void Inject<TInterface, TService>() where TInterface : class where TService : class, TInterface
        {
            _container.Register<TInterface, TService>();
        }
        
        public void Inject<TService>()  where TService : class
        {
            _container.Register<TService>();
        }
        
        public void Inject<TInterface, TService>(Lifestyle lifestyle) where TInterface : class where TService : class, TInterface
        {
            
            _container.Register<TInterface, TService>(lifestyle);
        }

        public void InjectServiceBase()
        {
            _container.Collection.Register(typeof(IServiceBase), typeof(IServiceBase).Assembly);
        }

        public void InjectAutoMapper()
        {
            // Assume we have multiple Profile classes.  We'll load them individually to create multiple mappers for our factory
            var mapperFactory = new MapperFactory();
            IMapper defaultMapper = null;
            var currentAssembly = Assembly.GetEntryAssembly();
            var types = currentAssembly.GetTypes().Where(x => x.GetTypeInfo().IsClass && x.IsAssignableFrom(x) && x.GetTypeInfo().BaseType == typeof(Profile)).ToList();
            foreach (var type in types)
            {
                var profileName = string.Empty;
                var config = new AutoMapper.MapperConfiguration(cfg =>
                {
                    var profile = (Profile)Activator.CreateInstance(type);
                    profileName = profile.ProfileName;
                    cfg.AddProfile(profile);
                });

                var mapper = config.CreateMapper();
                mapperFactory.Mappers.Add(profileName, mapper);

                // If we still want normal functionality with a default injected IMapper
                if (defaultMapper == null)
                {
                    defaultMapper = mapper;
                    _container.Register(() => defaultMapper);
                }
            }

            _container.Register<IMapperFactory>(() => mapperFactory);
        }
    }
}
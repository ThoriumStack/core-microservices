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
        private readonly IConfigurationRoot _configRoot;

        internal ServiceConfiguration(Container container, IConfigurationRoot configRoot)
        {
            _container = container;
            _configRoot = configRoot;
        }
        
        public void AddServiceEndpoint<TEndpoint>() where TEndpoint : class, IServiceEndpoint
        {
            _container.Collection.Append<IServiceEndpoint, TEndpoint>();            
        }

        public void AddConfiguration<TConfiguration>() where TConfiguration : class
        {
            var oType = typeof(TConfiguration);
            var isList =(oType.IsGenericType && (oType.GetGenericTypeDefinition() == typeof(List<>)));
            
            var typeName = typeof(TConfiguration).Name;
            if (isList)
            {
                typeName = oType.GenericTypeArguments.First().Name;
            }
            
            
            
            _container.Register(() => _configRoot.GetSection(typeName).Get<TConfiguration>());
        }

        public void Inject<TInterface, TService>() where TInterface : class where TService : class, TInterface
        {
            _container.Register<TInterface, TService>();
        }

        public void InjectServiceBase()
        {
            _container.Collection.Register(typeof(IServiceBase), typeof(IServiceBase).Assembly);
        }

        public void InjectAutoMapper(Assembly currentAssembly)
        {
            // Assume we have multiple Profile classes.  We'll load them individually to create multiple mappers for our factory
            var mapperFactory = new MapperFactory();
            IMapper defaultMapper = null;
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
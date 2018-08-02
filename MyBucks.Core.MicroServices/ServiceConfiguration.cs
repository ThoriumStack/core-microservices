using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using MyBucks.Core.MicroServices.Abstractions;
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
    }
}
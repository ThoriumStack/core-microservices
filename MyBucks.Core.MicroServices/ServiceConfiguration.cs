using Microsoft.Extensions.Configuration;
using MyBucks.Core.MicroServices.Abstractions;
using SimpleInjector;

namespace MyBucks.Core.MicroServices
{
    public class ServiceConfiguration
    {
        private Container _container;
        private IConfigurationRoot _configRoot;

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
            var typeName = typeof(TConfiguration).Name;
            
            _container.Register(() => _configRoot.GetSection(typeName).Get<TConfiguration>());
        }

        public void Inject<TInterface, TService>() where TInterface : class where TService : class, TInterface
        {
            _container.Register<TInterface, TService>();
        }
    }
}
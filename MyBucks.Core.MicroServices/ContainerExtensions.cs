using SimpleInjector;

namespace MyBucks.Core.MicroServices
{
    public static class ContainerExtensions
    {
        public static void InjectStartupInstance<T>(this Container destination) where T : class
        {
            var startupContainer = ServiceStartup.GetContainer();
            destination.Register(startupContainer.GetInstance<T>);
        }
    }
}
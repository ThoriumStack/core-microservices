using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using MyBucks.Core.MicroServices.Abstractions;
using SimpleInjector;

namespace MyBucks.Core.MicroServices
{
    public interface IServiceStartup
    {
        void LoadEndpoints(Container container);
        void RegisterStaticInstances(Container container);
        void RegisterServices(Container container);
        void LoadConfiguration(IConfigurationRoot configuration);
    }
}
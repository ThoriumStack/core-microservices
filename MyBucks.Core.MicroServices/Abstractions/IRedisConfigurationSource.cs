using System;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using MyBucks.Core.MicroServices.ConfigurationModels;
using MyBucks.Core.MicroServices.Redis;

namespace MyBucks.Core.MicroServices.Abstractions
{
    public interface IRedisConfigurationSource : IConfigurationSource
    {
        /// <summary>Used to access the contents of the file.</summary>
        IFileProvider FileProvider { get; set; }

        /// <summary>The path to the file.</summary>
        string Path { get; set; }

        /// <summary>Determines if loading the file is optional.</summary>
        bool Optional { get; set; }

        /// <summary>
        /// Determines whether the source will be loaded if the underlying file changes.
        /// </summary>
        bool ReloadOnChange { get; set; }

        RedisConfig RedisConfig { get; set; }
        
        CancellationToken CancellationToken { get; set; }

        Action<RedisWatchExceptionContext> OnWatchException { get; set;}

        Action OnReload { get; set; }
    }
}
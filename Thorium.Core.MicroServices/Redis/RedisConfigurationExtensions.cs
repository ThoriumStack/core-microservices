using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Thorium.Core.MicroServices.Abstractions;
using Thorium.Core.MicroServices.ConfigurationModels;

namespace Thorium.Core.MicroServices.Redis
{
    public static class RedisConfigurationExtensions
    {
        /*public static IConfigurationBuilder AddRedisConfig(this IConfigurationBuilder builder, RedisConfig redisConfig, string path)
        {
            return AddRedisConfig(builder, provider: null, redisConfig: redisConfig, path: path, optional: false, reloadOnChange: false);
        }

        public static IConfigurationBuilder AddRedisConfig(this IConfigurationBuilder builder, RedisConfig redisConfig, string path, bool optional)
        {
            return AddRedisConfig(builder, provider: null, redisConfig: redisConfig, path: path, optional: optional, reloadOnChange: false);
        }*/

        public static IConfigurationBuilder AddRedisConfig(this IConfigurationBuilder builder, RedisConfig redisConfig, string path, bool optional, bool reloadOnChange)
        {
            return AddRedisConfig(builder, provider: null, redisConfig: redisConfig, path: path, optional: optional, reloadOnChange: reloadOnChange);
        }

        public static IConfigurationBuilder AddRedisConfig(this IConfigurationBuilder builder, IFileProvider provider, RedisConfig redisConfig, string path, bool optional, bool reloadOnChange)
        {
            var cancellationToken = new CancellationToken();
            return AddRedisConfig(builder, cancellationToken, options =>
            {
                options.RedisConfig = redisConfig;
                options.FileProvider = provider;
                options.Path = path;
                options.Optional = optional;
                options.ReloadOnChange = reloadOnChange;
            });
        }

        public static IConfigurationBuilder AddRedisConfig(this IConfigurationBuilder builder, CancellationToken cancellationToken, Action<IRedisConfigurationSource> options)
        {
            var source = new RedisConfigurationSource(cancellationToken);
            options(source);
            
            if (source.FileProvider == null && Path.IsPathRooted(source.Path))
            {
                source.FileProvider = new PhysicalFileProvider(Path.GetDirectoryName(source.Path));
                source.Path = Path.GetFileName(source.Path);
            }
            
            builder.Add(source);
            return builder;
        }
    }
}
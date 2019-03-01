using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Primitives;
using StackExchange.Redis;
using Thorium.Core.MicroServices.Abstractions;

namespace Thorium.Core.MicroServices.Redis
{
    public class RedisConfigurationProvider : JsonConfigurationProvider
    {
        private readonly IRedisConfigurationSource _source;
        private readonly RedisConfigurationClient _redisConfigurationClient;
        private ConnectionMultiplexer redis;
        
        public RedisConfigurationProvider(RedisConfigurationSource source, RedisConfigurationClient client) :
            base(source)
        {
            _source = source;
            _redisConfigurationClient = client;

          
            
            if (!_source.RedisConfig.Upload && source.ReloadOnChange && source.FileProvider != null)
            {
                ChangeToken.OnChange(
                    () => _redisConfigurationClient.Watch(_source.OnWatchException),
                    async () =>
                    {
                        // Implement at later stage to do dynamic settings reloading
                        //LoadFile(reloading: true);
                        OnReload();
                        _source.OnReload?.Invoke();
                    });
            }
        }

        public IDictionary<string, string> ProviderData { get; set; }
        private string _configValueVersionPostfix = "_Version";

        public override void Load(Stream stream)
        {
            // Always create new Data on reload to drop old keys
            Data.Clear();
            
            // Load local appsetting.json into Data
            base.Load(stream);
            
            // Check setting and upload
            if (_source.RedisConfig.Upload)
            {
                if (redis == null || !redis.IsConnected)
                {
                    redis = ConnectionMultiplexer.Connect(_source.RedisConfig.Url);
                }
                Console.WriteLine($"Uploading config to Redis...");
                //var manager = new RedisManagerPool(_source.RedisConfig.Url);
                var client = redis.GetDatabase();
                {
                    foreach (var key in _source.RedisConfig.Keys)
                    {
                        int.TryParse(client.StringGet($"{key}{_configValueVersionPostfix}"), out int version);

                        client.StringSet($"{key}{_configValueVersionPostfix}", ++version);
                        
                        client.KeyDelete(key);
                        
                        foreach (var setting in Data)
                        {
                            client.HashSet(key, setting.Key, setting.Value);
                        }
                    }
                    Console.WriteLine($"Uploaded config to Redis...");
                }
            }
            else
            {
                Console.WriteLine($"Downloading config from Redis...");
                // Load setting from redis
                DoLoad(reloading: false).Wait();
                ProviderData = Data;
                Console.WriteLine($"Downloaded config from Redis...");
            }
        }

        private async Task DoLoad(bool reloading)
        {
            var configQueryResult = await _redisConfigurationClient.GetConfig();
            if (!configQueryResult.Exists && !_source.Optional)
            {
                if (!reloading)
                {
                    throw new Exception(
                        $"No corresponding key found in Redis {_source.RedisConfig.Keys.FirstOrDefault()}");
                }
                else
                {
                    return;
                }
            }

            LoadIntoMemory(configQueryResult);
        }

        private void LoadIntoMemory(ConfigQueryResult configQueryResult)
        {
            if (!configQueryResult.Exists)
            {
                Data.Clear();
            }
            else if (configQueryResult.Value != null)
            {
                foreach (var hashEntry in configQueryResult.Value)
                {
                    if (!Data.ContainsKey(hashEntry.Key))
                        Data.Add(hashEntry);
                }
            }
        }
    }
}
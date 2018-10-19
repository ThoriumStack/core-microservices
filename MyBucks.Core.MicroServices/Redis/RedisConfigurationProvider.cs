using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Primitives;
using MyBucks.Core.MicroServices.Abstractions;
using ServiceStack.Redis;

namespace MyBucks.Core.MicroServices.Redis
{
    public class RedisConfigurationProvider : JsonConfigurationProvider
    {
        private readonly IRedisConfigurationSource _source;
        private readonly RedisConfigurationClient _redisConfigurationClient;

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
                Console.WriteLine($"Uploading config to Redis...");
                var manager = new RedisManagerPool(_source.RedisConfig.Url);
                using (var client = manager.GetClient())
                {
                    foreach (var key in _source.RedisConfig.Keys)
                    {
                        var version = client.Get<int>($"{key}{_configValueVersionPostfix}");

                        client.Set($"{key}{_configValueVersionPostfix}", ++version);
                        
                        client.Remove(key);
                        
                        foreach (var setting in Data)
                        {
                            client.SetEntryInHash(key, setting.Key, setting.Value);
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
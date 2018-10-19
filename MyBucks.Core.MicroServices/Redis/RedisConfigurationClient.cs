using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using ServiceStack.Redis;

namespace MyBucks.Core.MicroServices.Redis
{
    public sealed class RedisConfigurationClient
    {
        private readonly object _lastVersionLock = new object();
        private int _lastVersion = 1;
        private RedisConfigurationSource _source;
        private string _configValueVersionPostfix = "_Version";

        private ConfigurationReloadToken _reloadToken = new ConfigurationReloadToken();

        public RedisConfigurationClient(RedisConfigurationSource source)
        {
            _source = source;
        }

        public async Task<ConfigQueryResult> GetConfig()
        {
            var result = await GetConfigValue();
            UpdateLastVersion(result);
            return result;
        }

        public IChangeToken Watch(Action<RedisWatchExceptionContext> onException)
        {
            Task.Run(() => PollForChanges(onException));
            return _reloadToken;
        }

        private async Task PollForChanges(Action<RedisWatchExceptionContext> onException)
        {
            while (!_source.CancellationToken.IsCancellationRequested)
            {
                try
                {
                    Console.WriteLine($"Polling Redis for changes...");
                    if (await HasValueChanged())
                    {
                        var previousToken = Interlocked.Exchange(ref _reloadToken, new ConfigurationReloadToken());
                        previousToken.OnReload();
                        return;
                    }
                }
                catch (Exception exception)
                {
                    var exceptionContext = new RedisWatchExceptionContext(_source, exception);
                    onException?.Invoke(exceptionContext);
                }

                await Task.Delay(new TimeSpan(0, _source.RedisConfig.PollingInterval, 0));
            }
        }

        private async Task<bool> HasValueChanged()
        {
            var queryResult = await GetConfigValue();
            return queryResult != null && UpdateLastVersion(queryResult);
        }

        private async Task<ConfigQueryResult> GetConfigValue() => await Task.Run(() =>
        {
            var manager = new RedisManagerPool(_source.RedisConfig.Url);
            using (var client = manager.GetClient())
            {
                var result = new ConfigQueryResult();

                foreach (var key in _source.RedisConfig.Keys)
                {
                    if (!client.ContainsKey(key))
                    {
                        result.Exists = false;
                        return result;
                    }

                    result.Exists = true;

                    //result.Value = client.Get<string>(key);
                    result.Value = client.GetAllEntriesFromHash(key);
                    result.Version = client.Get<int>($"{key}{_configValueVersionPostfix}");
                }

                return result;
            }
        });

        private bool UpdateLastVersion(ConfigQueryResult queryResult)
        {
            lock (_lastVersionLock)
            {
                if (queryResult.Version <= _lastVersion) return false;
                
                _lastVersion = queryResult.Version;
                return true;
            }
        }
    }
}
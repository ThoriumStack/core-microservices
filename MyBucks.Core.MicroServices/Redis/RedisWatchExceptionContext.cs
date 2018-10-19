using System;
using MyBucks.Core.MicroServices.Abstractions;

namespace MyBucks.Core.MicroServices.Redis
{
    public sealed class RedisWatchExceptionContext
    {
        internal RedisWatchExceptionContext(IRedisConfigurationSource source, Exception exception)
        {
            Source = source;
            Exception = exception;
        }

        public IRedisConfigurationSource Source { get; }

        public Exception Exception { get; }
    }
}
using System;
using Thorium.Core.MicroServices.Abstractions;

namespace Thorium.Core.MicroServices.Redis
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
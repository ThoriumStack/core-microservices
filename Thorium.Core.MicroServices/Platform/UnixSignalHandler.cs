using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Unix;

namespace Thorium.Core.MicroServices.Platform
{
    public class UnixSignalHandler
    {
        public void WaitForSignal(List<Mono.Unix.Native.Signum> signals, Action handle)
        {
            int index = UnixSignal.WaitAny(signals.Select(c => new UnixSignal(c)).ToArray(), -1);

            handle();
        }
    }
}
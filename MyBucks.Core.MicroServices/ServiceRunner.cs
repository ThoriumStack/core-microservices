using System;
using System.Runtime.InteropServices;
using System.Threading;
using MyBucks.Core.MicroServices.Abstractions;
using MyBucks.Core.MicroServices.Platform;

namespace MyBucks.Core.MicroServices
{
    public class ServiceRunner
    
    
    {
        private static ServiceStartup _startUp;

        public void Run(IServiceStartup startClass)
        {
            _startUp = new ServiceStartup(startClass);
            _startUp.Initialize();

            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            var handler = new UnixSignalHandler();

            if (!isWindows)
            {
                handler.WaitForSignal(new System.Collections.Generic.List<Mono.Unix.Native.Signum>
                {
                    Mono.Unix.Native.Signum.SIGQUIT,
                    Mono.Unix.Native.Signum.SIGTERM,
                    Mono.Unix.Native.Signum.SIGHUP
                }, Exit);
            }

            else
            {
                Console.CancelKeyPress += Console_CancelKeyPress;
                Thread.Sleep(Timeout.Infinite);
            }
        }
        
        private static void Console_CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Exit();
        }

        private static void Exit()
        {
            _startUp.StopServices();

            Environment.Exit(0);
        }
    }
}
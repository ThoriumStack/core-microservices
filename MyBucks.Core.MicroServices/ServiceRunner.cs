using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using MyBucks.Core.MicroServices.Abstractions;
using MyBucks.Core.MicroServices.Platform;

namespace MyBucks.Core.MicroServices
{
    public class ServiceRunner
    {
        private static ServiceStartup _startUp;
        private ServiceCommandLine _commandLineApp;
        
        public ServiceRunner()
        {
            _commandLineApp = new ServiceCommandLine(Assembly.GetExecutingAssembly().GetName().Name);
         var rc = new ReadyCheckCli();
            rc.ExtendCli(_commandLineApp.GetCommandLineApp());
        }

        public int Run(IServiceStartup startClass, string[] args)
        {
            if (args.Any())
            {
                return _commandLineApp.Run(args);
            }
            
            
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

            return 0;
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
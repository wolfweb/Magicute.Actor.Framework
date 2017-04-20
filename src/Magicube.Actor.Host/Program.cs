using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Orleans.Runtime.Configuration;

namespace Magicube.Actor.Host {
    class Program {
        static void Main(string[] args) {
            AppDomain hostDomain = AppDomain.CreateDomain("OrleansHost", null, new AppDomainSetup {
                AppDomainInitializer = InitSilo,
                AppDomainInitializerArguments = args,
            });

            Console.WriteLine("Orleans Silo is running.\nPress Enter to terminate...");
            Console.ReadLine();

            hostDomain.DoCallBack(ShutdownSilo);
        }

        static void InitSilo(string[] args) {
            _hostWrapper = new OrleansHostWrapper(args);
            _hostWrapper.OnInit += (sender, arg) => {
                var siloHost = (OrleansHostEventArgs)arg;
                if (File.Exists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OrleansConfiguration.xml"))) {
                    siloHost.Target.ConfigFileName = "OrleansConfiguration.xml";
                    siloHost.Target.LoadOrleansConfig();
                } else {
                    var config = ClusterConfiguration.LocalhostPrimarySilo();
                    siloHost.Target.Config = config;
                }
            };

            if (!_hostWrapper.Run()) {
                Console.Error.WriteLine("Failed to initialize Orleans silo");
            }
        }

        static void ShutdownSilo() {
            if (_hostWrapper != null) {
                _hostWrapper.Dispose();
                GC.SuppressFinalize(_hostWrapper);
            }
        }

        private static OrleansHostWrapper _hostWrapper;
    }
}

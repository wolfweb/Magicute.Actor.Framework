using System;
using System.Net;
using Orleans.Runtime.Configuration;
using Orleans.Runtime.Host;

namespace Magicube.Actor.Host {
    internal class OrleansHostEventArgs : EventArgs {
        public SiloHost Target { get; }

        public OrleansHostEventArgs(SiloHost siloHost) {
            Target = siloHost;
        }
    }

    internal class OrleansHostWrapper : IDisposable {
        public event EventHandler OnInit;

        public bool Debug
        {
            get { return _siloHost != null && _siloHost.Debug; }
            set { _siloHost.Debug = value; }
        }

        private SiloHost _siloHost;

        public OrleansHostWrapper(string[] args) {
            ParseArguments(args);
            Init();
        }

        public bool Run() {
            bool ok = false;

            try {
                OnInit?.Invoke(this, new OrleansHostEventArgs(_siloHost));

                _siloHost.InitializeOrleansSilo();

                ok = _siloHost.StartOrleansSilo();

                if (ok) {
                    Console.WriteLine("Successfully started Orleans silo '{0}' as a {1} node.", _siloHost.Name, _siloHost.Type);
                } else {
                    throw new SystemException($"Failed to start Orleans silo '{_siloHost.Name}' as a {_siloHost.Type} node.");
                }
            } catch (Exception exc) {
                _siloHost.ReportStartupError(exc);
                var msg = $"{exc.GetType().FullName}:\n{exc.Message}\n{exc.StackTrace}";
                Console.WriteLine(msg);
            }

            return ok;
        }

        public bool Stop() {
            bool ok = false;

            try {
                _siloHost.StopOrleansSilo();
                ok = true;
                Console.WriteLine("Orleans silo '{0}' shutdown.", _siloHost.Name);
            } catch (Exception exc) {
                _siloHost.ReportStartupError(exc);
                var msg = $"{exc.GetType().FullName}:\n{exc.Message}\n{exc.StackTrace}";
                Console.WriteLine(msg);
            }

            return ok;
        }

        private void Init() {
        }

        private bool ParseArguments(string[] args) {
            string deploymentId = null;
            string siloName = "Defaults";

            int argPos = 1;
            foreach (string a in args) {
                if (a.StartsWith("-") || a.StartsWith("/")) {
                    switch (a.ToLowerInvariant()) {
                        case "/?":
                        case "/help":
                        case "-?":
                        case "-help":
                            // Query usage help
                            return false;
                        default:
                            Console.WriteLine("Bad command line arguments supplied: " + a);
                            return false;
                    }
                }
                if (a.Contains("=")) {
                    string[] split = a.Split('=');
                    if (String.IsNullOrEmpty(split[1])) {
                        Console.WriteLine("Bad command line arguments supplied: " + a);
                        return false;
                    }
                    switch (split[0].ToLowerInvariant()) {
                        case "deploymentid":
                            deploymentId = split[1];
                            break;
                        default:
                            Console.WriteLine("Bad command line arguments supplied: " + a);
                            return false;
                    }
                } else if (argPos == 1) {
                    siloName = a;
                    argPos++;
                } else {
                    Console.WriteLine("Too many command line arguments supplied: " + a);
                    return false;
                }
            }
            
            _siloHost = new SiloHost(siloName);

            if (deploymentId != null)
                _siloHost.DeploymentId = deploymentId;

            return true;
        }

        public void PrintUsage() {
            Console.WriteLine(
                @"USAGE: 
    OrleansHost.exe [<siloName> [<configFile>]] [DeploymentId=<idString>] [/debug]
Where:
    <siloName>      - Name of this silo in the Config file list (optional)
    DeploymentId=<idString> 
                    - Which deployment group this host instance should run in (optional)
    /debug          - Turn on extra debug output during host startup (optional)");
        }

        public void Dispose() {
            Dispose(true);
        }

        protected virtual void Dispose(bool dispose) {
            _siloHost.Dispose();
            _siloHost = null;
        }
    }
}
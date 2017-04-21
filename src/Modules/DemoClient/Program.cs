using System;
using System.Collections.Generic;
using Magicube.Actor.Domain;
using Magicube.Actor.GrainInterfaces;
using Orleans;
using Orleans.Runtime.Configuration;

namespace DemoClient {
    class Program {
        private static NativeMethods.HandlerRoutine _rout;
        private static ICommandGrain<int> _commandGrain;
        private static IClientObserver _observer;
        private static ConnectRequest _request;
        private static readonly ClientCommandContext ClientCommandContext = new ClientCommandContext();

        static void Main(string[] args) {
            _rout = new NativeMethods.HandlerRoutine(ConsoleCtrlCheck);
            NativeMethods.WinNative.SetConsoleCtrlHandler(_rout, true);

            var config = ClientConfiguration.LoadFromFile("ClientConfiguration.xml");
            GrainClient.Initialize(config);
            var job = new DemoJobModel();
            _request = new ConnectRequest {
                Display = "Demo示例",
                ClientId = $"{job.JobDescription.JobGroup}-{job.JobDescription.JobName}",
                Description = job.JobDescription
            };

            _commandGrain = GetGrain<int>();
            var watcher = new DemoObserver(ClientCommandContext);
            _observer = GrainClient.GrainFactory.CreateObjectReference<IClientObserver>(watcher).Result;

            ClientCommandContext.Name = "connect";
            ClientCommandContext.ConnectContext = new ConnectContext {Connect = _request};
            ClientCommandContext.Observer = _observer;

            _commandGrain.Execute(ClientCommandContext).Wait();
            Console.WriteLine("connected...");
            Console.ReadKey();
        }

        public static ICommandGrain<T> GetGrain<T>() {
            return GrainClient.GrainFactory.GetGrain<ICommandGrain<T>>(0);
        }

        private static bool ConsoleCtrlCheck(NativeMethods.CtrlTypes type) {
            switch (type) {
                case NativeMethods.CtrlTypes.CTRL_C_EVENT:
                case NativeMethods.CtrlTypes.CTRL_BREAK_EVENT:
                case NativeMethods.CtrlTypes.CTRL_CLOSE_EVENT:
                case NativeMethods.CtrlTypes.CTRL_LOGOFF_EVENT:
                case NativeMethods.CtrlTypes.CTRL_SHUTDOWN_EVENT: {
                        ConsoleUtility.WriteLine("disconnected....", ConsoleColor.Red);
                        ClientCommandContext.Name = "disconnect";
                        _commandGrain.Execute(ClientCommandContext).Wait();
                    }
                    break;
            }
            return false;
        }
    }

    public class DemoObserver : IClientObserver {
        private readonly ClientCommandContext _ctx;

        public DemoObserver(ClientCommandContext ctx) {
            _ctx = ctx;
        }

        public void ExecuteCmd(CommandContext ctx) {
            switch (ctx.Name) {
                case "StartJob":
                    if (ctx.ArguementCtx != null)
                        foreach (KeyValuePair<string, object> item in ctx.ArguementCtx) {
                            Console.WriteLine($"key-{item.Key},value-{item.Value}");
                        } else
                        Console.WriteLine("start job");
                    break;
                case "RegisterReport":
                    Console.WriteLine("executed!");
                    break;
            }
        }
    }

    public class DemoJobModel {
        public JobDescription JobDescription => new JobDescription {
            JobGroup = "ChuyeUser",
            JobName = "DemoClient",
            Description = "Demo",
            Schedule = new SecondJobSchedule {
                IntervalStep = 5
            },
            JobData = new TransferContext().TryAdd(Guid.NewGuid().ToString("n"), Guid.NewGuid().ToString("n"))
        };
    }
}

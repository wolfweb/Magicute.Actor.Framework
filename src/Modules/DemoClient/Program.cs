using System;
using System.Collections;
using System.Collections.Generic;
using Magicube.Actor.Domain;
using Magicube.Actor.GrainInterfaces;
using Orleans;
using Orleans.Runtime.Configuration;

namespace DemoClient {
    class Program {
        private static NativeMethods.HandlerRoutine _rout;
        private static IConnectGrain _connectGrain;
        private static IConnectObserver _observer;
        private static ConnectRequest _request;

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

            _connectGrain = GrainClient.GrainFactory.GetGrain<IConnectGrain>(0);
            var watcher = new DemoObserver(_request, _connectGrain);
            _observer = GrainClient.GrainFactory.CreateObjectReference<IConnectObserver>(watcher).Result;
            var result = _connectGrain.Connect(_request, _observer).Result;
            Console.WriteLine(result);
            Console.ReadKey();
        }

        private static bool ConsoleCtrlCheck(NativeMethods.CtrlTypes type) {
            switch (type) {
                case NativeMethods.CtrlTypes.CTRL_C_EVENT:
                case NativeMethods.CtrlTypes.CTRL_BREAK_EVENT:
                case NativeMethods.CtrlTypes.CTRL_CLOSE_EVENT:
                case NativeMethods.CtrlTypes.CTRL_LOGOFF_EVENT:
                case NativeMethods.CtrlTypes.CTRL_SHUTDOWN_EVENT:
                    ConsoleUtility.WriteLine("disconnected....", ConsoleColor.Red);
                    _connectGrain.DisConnect(_request.ClientId).Wait();
                    break;
            }
            return false;
        }
    }

    public class DemoObserver : IConnectObserver {
        private readonly ConnectRequest _request;
        private readonly IConnectGrain _connectGrain;

        public DemoObserver(ConnectRequest request, IConnectGrain connectGrain) {
            _request = request;
            _connectGrain = connectGrain;
        }

        public void ExecuteCmd(CmdContext ctx) {
            switch (ctx.Message) {
                case JobNoticeMsg.ExecuteJob:
                    if (ctx.ArguementContext != null)
                        foreach (KeyValuePair<string, object> item in ctx.ArguementContext) {
                            Console.WriteLine($"key-{item.Key},value-{item.Value}");
                        } else
                        Console.WriteLine("start job");
                    break;
                case JobNoticeMsg.ExecuteRepair:
                    _connectGrain.RepairReport(_request.ClientId, ctx.ArguementContext.ToString()).Wait();
                    break;
                case JobNoticeMsg.RegisterReport:
                    Console.WriteLine("executed!");
                    break;
                case JobNoticeMsg.Ping:
                    _connectGrain.Pong(_request.ClientId).Wait();
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

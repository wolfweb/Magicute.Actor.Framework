using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Magicube.Actor.Domain;
using Magicube.Actor.GrainInterfaces;
using Microsoft.AspNet.SignalR.Client;
using Orleans;
using Orleans.Concurrency;

namespace Magicube.Actor.Implementations
{
    internal class InnerPools {
        private static readonly ConcurrentDictionary<string, ConnectContext> Clients = new ConcurrentDictionary<string, ConnectContext>();

        public static void Enqueue(IConnectObserver observer, ConnectRequest request) {
            Clients.AddOrUpdate(request.ClientId, new ConnectContext {
                ClientId = request.ClientId,
                Client = request,
                Observer = observer
            }, (k, v) => new ConnectContext {
                ClientId = request.ClientId,
                Client = request,
                Observer = observer
            });
        }

        public static ConnectContext Dequeue(string clientId) {
            ConnectContext item;
            Clients.TryRemove(clientId, out item);
            return item;
        }

        public static List<ConnectContext> Gets() {
            return Clients.Values.ToList();
        }

        public static ConnectContext GetContext(string clientId) {
            var ctx = Clients.FirstOrDefault(m => m.Key == clientId);
            if (!string.IsNullOrEmpty(ctx.Key))
                return ctx.Value;
            return null;
        }
    }

    [Reentrant]
    public class ConnectGrain : Grain, IConnectGrain, IReportGrain {
        private readonly Dictionary<string, Tuple<HubConnection, IHubProxy>> _hubs = new Dictionary<string, Tuple<HubConnection, IHubProxy>>();
        private static ObserverSubscriptionManager<IConnectObserver> _subscribers;
        private static ObserverSubscriptionManager<IManagerObserver> _subscriptionManager;

        public override async Task OnActivateAsync() {
            _subscribers = new ObserverSubscriptionManager<IConnectObserver>();
            _subscriptionManager = new ObserverSubscriptionManager<IManagerObserver>();
            await base.OnActivateAsync();
        }

        public override async Task OnDeactivateAsync() {
            _subscribers.Clear();
            _subscriptionManager.Clear();
            await TaskDone.Done;
        }

        public async Task<bool> Connect(ConnectRequest request, IConnectObserver subscriber) {
            InnerPools.Enqueue(subscriber, request);
            if (!_subscribers.IsSubscribed(subscriber))
                _subscribers.Subscribe(subscriber);

            await OnConnected(request);

            return await Task.FromResult(true);
        }

        public async Task DisConnect(string clientId) {
            var ctx = InnerPools.GetContext(clientId);
            if (ctx != null) {
                _subscribers.Unsubscribe(ctx.Observer);
                await OnDisConnected(ctx);
            }

            InnerPools.Dequeue(clientId);
            await TaskDone.Done;
        }

        public async Task ExecuteCmd(string clientId, CmdContext ctx) {
            var connectCtx = InnerPools.GetContext(clientId);
            connectCtx?.Observer.ExecuteCmd(ctx);
            await Task.FromResult(0);
        }

        public async Task Manage(IManagerObserver subscriber) {
            _subscriptionManager.Subscribe(subscriber);
            await TaskDone.Done;
        }

        public async Task Ping(string clientId) {
            _subscribers.Notify(x => {
                var ctx = InnerPools.GetContext(clientId);
                ctx?.Observer.ExecuteCmd(new CmdContext {
                    Message = JobNoticeMsg.Ping,
                    ArguementContext = new TransferContext().TryAdd("id", clientId)
                });
            });
            await TaskDone.Done;
        }

        public async Task Pong(string clientId) {
            foreach (var hub in _hubs.Values) {
                try {
                    if (hub.Item1.State != ConnectionState.Connected) {
                        await hub.Item1.Start();
                    }
                    if (hub.Item1.State == ConnectionState.Connected) {
                        await hub.Item2.Invoke("Pong", clientId);
                    }
                } catch (Exception ex) {
                    Console.WriteLine(ex);
                }
            }

            await TaskDone.Done;
        }

        public async Task RepairReport(string clientId, string message) {
            foreach (var hub in _hubs.Values) {
                try {
                    if (hub.Item1.State != ConnectionState.Connected) {
                        await hub.Item1.Start();
                    }
                    if (hub.Item1.State == ConnectionState.Connected) {
                        await hub.Item2.Invoke("RepairReport", clientId, message);
                    }
                } catch (Exception ex) {
                    Console.WriteLine(ex);
                }
            }

            await TaskDone.Done;
        }

        public async Task RegisteHub(string address, string hubName) {
            if (!_hubs.ContainsKey(address)) {
                var hubConnection = new HubConnection(address);
                hubConnection.Headers.Add("ORLEANS", "GRAIN");
                var hub = hubConnection.CreateHubProxy(hubName);
                await hubConnection.Start();
                _hubs.Add(address, new Tuple<HubConnection, IHubProxy>(hubConnection, hub));
            }
        }

        #region Report
        public async Task NotifyReport(string clientId, ReportConfig config) {
            var ctx = InnerPools.GetContext(clientId);
            if (ctx != null) {
                var transferCtx = new TransferContext();
                ctx.Observer.ExecuteCmd(new CmdContext {
                    Message = JobNoticeMsg.RegisterReport,
                    ArguementContext = transferCtx.TryAdd("model", config)
                });
                await Task.FromResult(0);
            }
        }

        public async Task BeforeReport(DateTime curTime, int id) {
            foreach (var hub in _hubs.Values) {
                try {
                    if (hub.Item1.State != ConnectionState.Connected) {
                        await hub.Item1.Start();
                    }
                    if (hub.Item1.State == ConnectionState.Connected) {
                        await hub.Item2.Invoke("BeforeReport", curTime, id);
                    }
                } catch (Exception ex) {
                    Console.WriteLine(ex);
                }
            }

            await TaskDone.Done;
        }

        #endregion

        private async Task OnConnected(ConnectRequest request) {
            _subscriptionManager.Notify(x => {
                var ctx = InnerPools.GetContext(request.ClientId);
                if (ctx != null)
                    x.UpdateJob(ctx);
            });

            foreach (var hub in _hubs.Values) {
                try {
                    if (hub.Item1.State != ConnectionState.Connected) {
                        await hub.Item1.Start();
                    }
                    if (hub.Item1.State == ConnectionState.Connected) {
                        await hub.Item2.Invoke("ComponentUpdate", new ClientStatus { ClientId = request.ClientId, State = ClientState.Running });
                    }
                } catch (Exception ex) {
                    Console.WriteLine(ex);
                }
            }

            await TaskDone.Done;
        }

        private async Task OnDisConnected(ConnectContext ctx) {
            _subscriptionManager.Notify(x => {
                x.StopJob(ctx);
            });

            foreach (var hub in _hubs.Values) {
                try {
                    if (hub.Item1.State != ConnectionState.Connected) {
                        await hub.Item1.Start();
                    }
                    if (hub.Item1.State == ConnectionState.Connected) {
                        await hub.Item2.Invoke("ComponentUpdate", new ClientStatus { ClientId = ctx.ClientId, State = ClientState.Stop });
                    }
                } catch (Exception ex) {
                    Console.WriteLine(ex);
                }
            }

            await TaskDone.Done;
        }
    }

    [Reentrant]
    public class ManageGrain : Grain, IManageGrain {
        public async Task<List<ConnectContext>> GetClients() {
            return await Task.FromResult(InnerPools.Gets().ToList());
        }
    }
}

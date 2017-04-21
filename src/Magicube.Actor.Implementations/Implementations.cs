using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Magicube.Actor.GrainInterfaces;
using Microsoft.AspNet.SignalR.Client;
using Orleans;
using Orleans.Concurrency;

namespace Magicube.Actor.Implementations {
    [Reentrant]
    public class CommandGrain<T> : Grain, ICommandGrain<T> {
        private readonly Dictionary<string, Tuple<HubConnection, IHubProxy>> _hubs = new Dictionary<string, Tuple<HubConnection, IHubProxy>>();
        private static ObserverSubscriptionManager<IClientObserver> _subscribers;
        private static ObserverSubscriptionManager<IJobObserver> _subscriptionJob;
        private readonly CommandFactory _commandFactory = new CommandFactory();

        public override async Task OnActivateAsync() {
            _subscribers = new ObserverSubscriptionManager<IClientObserver>();
            _subscriptionJob = new ObserverSubscriptionManager<IJobObserver>();
            await base.OnActivateAsync();
        }

        public override async Task OnDeactivateAsync() {
            _subscribers.Clear();
            _subscriptionJob.Clear();
            await TaskDone.Done;
        }

        public async Task<T> Execute(CommandContext cmd) {
            GrainContext ctx = BuildContext(cmd);
            return await _commandFactory.Execute<T>(ctx);
        }

        private GrainContext BuildContext(CommandContext cmd) {
            return new GrainContext {
                CommandContext = cmd,
                Hubs = _hubs,
                Subscribers = _subscribers,
                SubscriberJob = _subscriptionJob
            };
        }
    }
}

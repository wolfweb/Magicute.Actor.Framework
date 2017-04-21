using System;
using System.Collections.Generic;
using Magicube.Actor.GrainInterfaces;
using Microsoft.AspNet.SignalR.Client;
using Orleans;

namespace Magicube.Actor.Implementations{
    public class GrainContext {
        public ObserverSubscriptionManager<IClientObserver> Subscribers        { get; set; }
        public ObserverSubscriptionManager<IJobObserver> SubscriberJob         { get; set; }
        public Dictionary<string, Tuple<HubConnection, IHubProxy>> Hubs        { get; set; }
        public CommandContext CommandContext                                   { get; set; }
    }
}
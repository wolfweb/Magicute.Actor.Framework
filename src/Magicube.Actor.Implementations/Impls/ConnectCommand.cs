using System;
using System.Threading.Tasks;
using Magicube.Actor.Domain;
using Magicube.Actor.GrainInterfaces;
using Magicube.Actor.Implementations.Attributes;
using Magicube.Actor.Implementations.Interfaces;
using Microsoft.AspNet.SignalR.Client;
using Orleans;

namespace Magicube.Actor.Implementations.Impls {
    [Command("connect")]
    public class ConnectCommand : IAskCommand {
        public async Task Run(GrainContext arg) {
            var clientCmdCtx = arg.CommandContext as ClientCommandContext;
            var subscriber = clientCmdCtx?.Observer;
            if (subscriber != null) {
                InnerPools.Enqueue(subscriber, clientCmdCtx);
                if (!arg.Subscribers.IsSubscribed(subscriber))
                    arg.Subscribers.Subscribe(subscriber);
                await OnConnected(arg);
            }
        }

        private async Task OnConnected(GrainContext arg) {
            var request = arg.CommandContext.ConnectContext.Connect;
            arg.SubscriberJob.Notify(x => {
                x.RegisterJob(arg.CommandContext as ClientCommandContext);
            });

            foreach (var hub in arg.Hubs.Values) {
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
    }
}

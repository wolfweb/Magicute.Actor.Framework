using System;
using System.Threading.Tasks;
using Magicube.Actor.Domain;
using Magicube.Actor.GrainInterfaces;
using Magicube.Actor.Implementations.Attributes;
using Magicube.Actor.Implementations.Interfaces;
using Microsoft.AspNet.SignalR.Client;
using Orleans;

namespace Magicube.Actor.Implementations.Impls {
    [Command("disconnect")]
    public class DisConnectCommand : IAskCommand {
        public async Task Run(GrainContext arg) {
            var clientCmdCtx = arg.CommandContext as ClientCommandContext;
            var subscriber = clientCmdCtx?.Observer;
            if (subscriber != null) {
                arg.Subscribers.Unsubscribe(subscriber);
                await OnDisConnected(arg);

                InnerPools.Dequeue(subscriber.GetPrimaryKey().ToString("n"));
            }
        }

        private async Task OnDisConnected(GrainContext arg) {
            arg.SubscriberJob.Notify(x => {
                x.UnRegisterJob(arg.CommandContext as ClientCommandContext);
            });

            foreach (var hub in arg.Hubs.Values) {
                try {
                    if (hub.Item1.State != ConnectionState.Connected) {
                        await hub.Item1.Start();
                    }
                    if (hub.Item1.State == ConnectionState.Connected) {
                        await hub.Item2.Invoke("ComponentUpdate", new ClientStatus {
                            ClientId = arg.CommandContext.ConnectContext.Connect.ClientId,
                            State = ClientState.Stop
                        });
                    }
                } catch (Exception ex) {
                    Console.WriteLine(ex);
                }
            }

            await TaskDone.Done;
        }
    }
}

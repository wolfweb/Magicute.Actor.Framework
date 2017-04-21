using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Magicube.Actor.GrainInterfaces;
using Orleans;

namespace Magicube.Actor.Implementations {
    internal class InnerPools {
        private static readonly ConcurrentDictionary<string, ClientCommandContext> Clients = new ConcurrentDictionary<string, ClientCommandContext>();

        public static void Enqueue(IClientObserver observer, ClientCommandContext ctx) {
            Clients.AddOrUpdate(observer.GetPrimaryKey().ToString("n"), ctx, (k, v) => ctx);
        }

        public static ClientCommandContext Dequeue(string clientId) {
            ClientCommandContext item;
            Clients.TryRemove(clientId, out item);
            return item;
        }

        public static List<ClientCommandContext> Gets() {
            return Clients.Values.ToList();
        }

        //public static ClientCommandContext GetContext(string clientId) {
        //    var ctx = Clients.FirstOrDefault(m => m.Key == clientId);
        //    if (!string.IsNullOrEmpty(ctx.Key))
        //        return ctx.Value;
        //    return null;
        //}
    }
}
using System;
using Magicube.Actor.Domain;

namespace Magicube.Actor.GrainInterfaces {
    public class CommandContext {
        protected CommandContext() {
            ArguementCtx = new TransferContext();
            ConnectContext = new ConnectContext();
        }
        public string Name { get; set; }
        public TransferContext ArguementCtx { get; set; }
        public ConnectContext ConnectContext { get; set; }
    }

    [Serializable]
    public class ClientCommandContext : CommandContext {
        public IClientObserver Observer { get; set; }
    }

    [Serializable]
    public class JobCommandContext : CommandContext {
        public IJobObserver Observer { get; set; }
    }
}
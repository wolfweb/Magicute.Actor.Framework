using System;
using Magicube.Actor.Domain;

namespace Magicube.Actor.GrainInterfaces {
    [Serializable]
    public class ConnectContext {
        public string ClientId { get; set; }
        public ClientState State { get; set; }
        public ConnectRequest Client { get; set; }
        public IConnectObserver Observer { get; set; }
    }
}
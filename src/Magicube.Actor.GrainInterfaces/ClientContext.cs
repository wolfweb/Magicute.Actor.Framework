using System;
using Magicube.Actor.Domain;

namespace Magicube.Actor.GrainInterfaces {
    [Serializable]
    public class ConnectContext {
        public ConnectRequest Connect { get; set; }
    }
}
using System;

namespace Magicube.Actor.Domain {
    [Serializable]
    public class ConnectRequest {
        public string ClientId { get; set; }
        public string Display { get; set; }
        public JobDescription Description { get; set; }
    }
}
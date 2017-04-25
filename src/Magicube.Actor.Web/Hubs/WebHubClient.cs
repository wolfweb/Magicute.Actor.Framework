using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Magicube.Actor.Web.Hubs {
    [HubName("WebHubClient")]
    public class WebHubClient : Hub {
        private const string WebIdentity = "BROWSERS";



        public override System.Threading.Tasks.Task OnConnected() {
            if (Context.Headers.Get("ORLEANS") != "GRAIN") {
                Groups.Add(Context.ConnectionId, WebIdentity);
            }
            return base.OnConnected();
        }
    }
}
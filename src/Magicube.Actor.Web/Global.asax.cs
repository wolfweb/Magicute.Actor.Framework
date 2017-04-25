using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Security;
using System.Web.SessionState;
using Orleans;
using Orleans.Runtime.Configuration;

namespace Magicube.Actor.Web {
    public class Global : System.Web.HttpApplication {

        protected void Application_Start(object sender, EventArgs e) {
            TryInitGrainClient();


        }

        protected void Session_Start(object sender, EventArgs e) {

        }

        protected void Application_BeginRequest(object sender, EventArgs e) {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e) {

        }

        protected void Application_Error(object sender, EventArgs e) {

        }

        protected void Session_End(object sender, EventArgs e) {

        }

        protected void Application_End(object sender, EventArgs e) {

        }

        private void TryInitGrainClient() {
            try {
                var file = HostingEnvironment.IsHosted ? HostingEnvironment.MapPath("~/ClientConfiguration.xml") : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ClientConfiguration.xml");
                var config = File.Exists(file) ? ClientConfiguration.LoadFromFile(file) : ClientConfiguration.LocalhostSilo();
                GrainClient.Initialize(config);
            } catch {
                //todo: notify event
            }
        }
    }
}
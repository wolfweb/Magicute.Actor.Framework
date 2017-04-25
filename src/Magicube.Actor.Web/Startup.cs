using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(Magicube.Actor.Web.Startup))]
namespace Magicube.Actor.Web {
    public class Startup {
        public void Configuration(IAppBuilder app) {
            app.MapSignalR();
        }
    }
}
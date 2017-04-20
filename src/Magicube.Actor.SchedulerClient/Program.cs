using Autofac;
using Magicube.Actor.SchedulerClient.Services;
using Topshelf;
using Topshelf.Autofac;

namespace Magicube.Actor.SchedulerClient {
    class Program {
        private const string ServiceServiceName = @"magicube-actor-schedulerclient";
        private const string ServiceDisplayName = @"magicube actor schedulerclient";
        private const string ServiceDescription = @"magicube actor schedulerclient";

        static void Main(string[] args) {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new SchedulerModule(args));
            var container = builder.Build();
            var host = HostFactory.New(x => {
                x.UseNLog();
                x.UseAutofacContainer(container);
                x.Service<SchedulerService>(s => {
                    s.ConstructUsingAutofacContainer();
                    s.WhenStarted((service, hc) => service.Start(hc));
                    s.WhenStopped((service, hc) => {
                        container.Dispose();
                        return service.Stop(hc);
                    });
                });

                x.SetServiceName(ServiceServiceName);
                x.SetDisplayName(ServiceDisplayName);
                x.SetDescription(ServiceDescription);

                x.RunAsLocalService();
                x.StartAutomatically();

                x.EnableServiceRecovery(src => {
                    src.RestartService(0);
                    src.OnCrashOnly();
                    src.SetResetPeriod(1);
                });
            });
            host.Run();
        }
    }
}

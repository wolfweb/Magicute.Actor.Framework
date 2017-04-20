using System;
using Autofac;
using Autofac.Core;
using Autofac.Extras.NLog;
using Autofac.Extras.Quartz;
using Magicube.Actor.SchedulerClient.Security;
using Magicube.Actor.SchedulerClient.Services;

namespace Magicube.Actor.SchedulerClient
{
    public class SchedulerModule : Module {
        private readonly string[] _args;

        public SchedulerModule(string[] args) {
            _args = args;
        }

        protected override void Load(ContainerBuilder builder) {
            builder.RegisterModule<NLogModule>();
            builder.RegisterType<SchedulerService>().SingleInstance();
            builder.RegisterType<AuthenticationService>().As<IAuthenticationService>()
                .WithParameter("sectionName", "Manager")
                .SingleInstance();
            builder.RegisterType<JobHostService>().As<IJobHostService>().WithParameter("args", _args);
            builder.RegisterModule(new QuartzAutofacFactoryModule());
            builder.RegisterModule(new QuartzAutofacJobsModule(typeof(SchedulerModule).Assembly));
            base.Load(builder);
        }
    }
}
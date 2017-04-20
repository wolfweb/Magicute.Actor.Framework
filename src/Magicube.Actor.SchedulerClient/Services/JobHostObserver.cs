using System;
using Magicube.Actor.Domain;
using Magicube.Actor.GrainInterfaces;
using Magicube.Actor.SchedulerClient.Modules;
using Orleans;
using Quartz;

namespace Magicube.Actor.SchedulerClient.Services {
    public class JobHostObserver : IManagerObserver {
        private readonly IScheduler _scheduler;
        private readonly IConnectGrain _connectGrain;

        public JobHostObserver() { }

        public JobHostObserver(IScheduler scheduler, IConnectGrain connectGrain) {
            _scheduler = scheduler;
            _connectGrain = connectGrain;
        }

        public void StopJob(ConnectContext ctx) {
            if (ctx.Client.Description == null) return;
            var jobKey = new JobKey(ctx.Client.Description.JobName, ctx.Client.Description.JobGroup);
            _scheduler.DeleteJob(jobKey);
            ConsoleUtility.WriteLine($"remove job {ctx.Client.Description.JobName}-{ctx.Client.Description.JobGroup} from {ctx.Observer.GetPrimaryKeyString()}", ConsoleColor.Red);
        }

        public void UpdateJob(ConnectContext ctx) {
            if (ctx.Client.Description == null) return;
            var jobKey = new JobKey(ctx.Client.Description.JobName, ctx.Client.Description.JobGroup);
            var job = new CastingJobDescription(ctx.Client.Description);
            Console.WriteLine("update");
            if (_scheduler.CheckExists(jobKey))
                _scheduler.DeleteJob(jobKey);

            _scheduler.ScheduleJob(job.RetrieveJobDetail(jobData => {
                _connectGrain.ExecuteCmd(ctx.ClientId, new CmdContext {
                    Message = JobNoticeMsg.ExecuteJob,
                    ArguementContext = jobData
                }).Wait();
            }), job.RetrieveJobTrigger());

            ConsoleUtility.WriteLine($"Job {ctx.Client.Description.JobGroup}-{ctx.Client.Description.JobName} Registed!", ConsoleColor.Green);
        }
    }
}
using System;
using Magicube.Actor.GrainInterfaces;
using Magicube.Actor.SchedulerClient.Modules;
using Orleans;
using Quartz;

namespace Magicube.Actor.SchedulerClient.Services {
    public class JobHostObserver : IJobObserver {
        private readonly IScheduler _scheduler;
        private readonly JobCommandContext _jobCtx;
        private ICommandGrain<int> ConnectGrain { get; }

        public JobHostObserver(IScheduler scheduler, JobCommandContext jobCtx, ICommandGrain<int> connectGrain) {
            _scheduler = scheduler;
            _jobCtx = jobCtx;
            ConnectGrain = connectGrain;
        }

        public void UnRegisterJob(ClientCommandContext ctx) {
            var jobCommandCtx = ctx as ClientCommandContext;
            if (jobCommandCtx == null) return;

            var connect = ctx.ConnectContext.Connect;
            if (connect.Description == null) return;
            var jobKey = new JobKey(connect.Description.JobName, connect.Description.JobGroup);
            _scheduler.DeleteJob(jobKey);

            ConsoleUtility.WriteLine($"remove job {connect.Description.JobName}-{connect.Description.JobGroup} from {jobCommandCtx.Observer.GetPrimaryKey().ToString("n")}", ConsoleColor.Red);
        }

        public void RegisterJob(ClientCommandContext ctx) {
            var jobCommandCtx = ctx as ClientCommandContext;
            if (jobCommandCtx == null) return;
            var connect = ctx.ConnectContext.Connect;
            if (connect.Description == null) return;
            var jobKey = new JobKey(connect.Description.JobName, connect.Description.JobGroup);
            var job = new CastingJobDescription(connect.Description);
            Console.WriteLine("update");
            if (_scheduler.CheckExists(jobKey))
                _scheduler.DeleteJob(jobKey);

            _scheduler.ScheduleJob(job.RetrieveJobDetail(jobData => {
                var cmd = new JobCommandContext {
                    Name = "startjob",
                    ArguementCtx = jobData,
                    Observer = _jobCtx.Observer
                };
                ConnectGrain.Execute(cmd).Wait();
            }), job.RetrieveJobTrigger());

            ConsoleUtility.WriteLine($"Job {connect.Description.JobGroup}-{connect.Description.JobName} Registed!", ConsoleColor.Green);
        }
    }
}
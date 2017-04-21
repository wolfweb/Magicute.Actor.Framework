using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Magicube.Actor.GrainInterfaces;
using Magicube.Actor.SchedulerClient.Modules;
using Orleans;
using Orleans.Runtime.Configuration;
using Quartz;

namespace Magicube.Actor.SchedulerClient.Services {
    public interface IJobHostService {
        Task StartHost(IScheduler scheduler);
        Task StopHost();
    }
    public class JobHostService : IJobHostService {
        private static readonly JobCommandContext JobCtx = new JobCommandContext();

        public JobHostService() {
            var config = ClientConfiguration.LoadFromFile("ClientConfiguration.xml");
            GrainClient.Initialize(config);
        }

        public async Task StartHost(IScheduler scheduler) {
            var connectGrain = GetGrain<int>();
            var watcher = new JobHostObserver(scheduler, JobCtx, connectGrain);
            var observer = await GrainClient.GrainFactory.CreateObjectReference<IJobObserver>(watcher);
            await connectGrain.Execute(new JobCommandContext { Name = "job-manage-connect", Observer = observer });

            JobCtx.Observer = observer;

            var manageGrain = GetGrain<List<ClientCommandContext>>();
            var clients = await manageGrain.Execute(new JobCommandContext { Name = "job-manage-getlist", Observer = observer });
            foreach (var client in clients) {
                var connect = client.ConnectContext.Connect;
                if (connect.Description == null) continue;
                var job = new CastingJobDescription(connect.Description);
                if (!scheduler.CheckExists(new JobKey(connect.Description.JobName, connect.Description.JobGroup))) {
                    scheduler.ScheduleJob(job.RetrieveJobDetail(jobData => {
                        var cmd = new ClientCommandContext {
                            Name = "job-manage-start",
                            ArguementCtx = jobData,
                            ConnectContext = client.ConnectContext,
                            Observer = client.Observer
                        };
                        manageGrain.Execute(cmd).Wait();
                    }), job.RetrieveJobTrigger());
                    ConsoleUtility.WriteLine($"Job {connect.Description.JobGroup}-{connect.Description.JobName} Registed!", ConsoleColor.Green);
                } else {
                    ConsoleUtility.WriteLine($"Job {connect.Description.JobGroup}-{connect.Description.JobName} ReConnectioned!", ConsoleColor.Green);
                }
            }
        }

        public async Task StopHost() {
            await TaskDone.Done;
        }


        public ICommandGrain<T> GetGrain<T>() {
            return GrainClient.GrainFactory.GetGrain<ICommandGrain<T>>(0);
        }
    }
}

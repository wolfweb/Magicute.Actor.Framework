using System;
using System.Threading.Tasks;
using Magicube.Actor.Domain;
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
        private readonly IConnectGrain _connectGrain;
        private readonly IManageGrain _manageGrain;

        public JobHostService() {
            var config = ClientConfiguration.LoadFromFile("ClientConfiguration.xml");
            GrainClient.Initialize(config);
            _connectGrain = GrainClient.GrainFactory.GetGrain<IConnectGrain>(0);
            _manageGrain = GrainClient.GrainFactory.GetGrain<IManageGrain>(0);
        }

        public async Task StartHost(IScheduler scheduler) {
            var watcher = new JobHostObserver(scheduler, _connectGrain);
            var observer = await GrainClient.GrainFactory.CreateObjectReference<IManagerObserver>(watcher);
            await _connectGrain.Manage(observer);
            var clients = await _manageGrain.GetClients();
            foreach (var client in clients) {
                if (client.Client.Description == null) continue;
                var job = new CastingJobDescription(client.Client.Description);
                if (!scheduler.CheckExists(new JobKey(client.Client.Description.JobName, client.Client.Description.JobGroup))) {
                    scheduler.ScheduleJob(job.RetrieveJobDetail(jobData => {
                        _connectGrain.ExecuteCmd(client.ClientId, new CmdContext {
                            Message = JobNoticeMsg.ExecuteJob,
                            ArguementContext = jobData
                        }).Wait();
                    }), job.RetrieveJobTrigger());
                    ConsoleUtility.WriteLine($"Job {client.Client.Description.JobGroup}-{client.Client.Description.JobName} Registed!", ConsoleColor.Green);
                } else {
                    ConsoleUtility.WriteLine($"Job {client.Client.Description.JobGroup}-{client.Client.Description.JobName} ReConnectioned!", ConsoleColor.Green);
                }
            }
        }

        public async Task StopHost() {
            await TaskDone.Done;
        }
    }
}

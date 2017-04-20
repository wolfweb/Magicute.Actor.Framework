using System;
using Autofac.Extras.NLog;
using Quartz;
using Topshelf;

namespace Magicube.Actor.SchedulerClient.Services {
    public class SchedulerService : ServiceControl {
        private readonly ILogger _logger;
        private readonly IScheduler _scheduler;
        private readonly IJobHostService _jobHostService;

        public SchedulerService(ILogger logger, IScheduler scheduler, IJobHostService jobHostService) {
            _logger = logger;
            _scheduler = scheduler;
            _jobHostService = jobHostService;
        }


        public bool Start(HostControl hostControl) {
            _logger.Info("scheduler service starting");

            if (!_scheduler.IsStarted) {
                _scheduler.Start();
            }

            try {
                _jobHostService.StartHost(_scheduler);
            } catch (Exception exp) {
                _logger.Error(exp);
            }

            return _scheduler.IsStarted;
        }

        public bool Stop(HostControl hostControl) {
            _logger.Info("scheduler service stopping");
            _scheduler.Shutdown(true);
            _jobHostService.StopHost();
            return _scheduler.IsShutdown;
        }
    }
}

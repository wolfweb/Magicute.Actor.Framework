using System;
using Autofac.Extras.NLog;
using Magicube.Actor.Domain;
using Magicube.Actor.GrainInterfaces;
using Polly;
using Quartz;

namespace Magicube.Actor.SchedulerClient.Jobs {
    public class JobsHandler : IJob {
        private readonly ILogger _logger;

        public JobsHandler(ILogger logger) {
            _logger = logger;
        }

        public void Execute(IJobExecutionContext context) {
            var desc = context.MergedJobDataMap["Desc"];
            if (desc != null)
                ConsoleUtility.WriteLine($"Start Job {desc} At {DateTime.Now}", ConsoleColor.Red);
            Action<TransferContext> handler = (Action<TransferContext>)context.MergedJobDataMap["Action"];

            var policy = Policy.Handle<Exception>()
                .WaitAndRetry(new[] { TimeSpan.FromSeconds(3) }, (ex, timeSpan, ctx) => {
                    _logger.Error(ex);
                });

            var transferCtx = new TransferContext();
            foreach (var item in context.MergedJobDataMap) {
                transferCtx.TryAdd(item.Key, item.Value);
            }

            policy.Execute(() => handler(transferCtx));
        }
    }
}

using System.Threading.Tasks;
using Magicube.Actor.GrainInterfaces;
using Magicube.Actor.Implementations.Attributes;
using Magicube.Actor.Implementations.Interfaces;
using Orleans;

namespace Magicube.Actor.Implementations.Impls {
    [Command("job-manage-connect")]
    public class JobConnectCommand : IAskCommand {
        public async Task Run(GrainContext arg) {
            var cmdCtx = (JobCommandContext)arg.CommandContext;
            arg.SubscriberJob.Subscribe(cmdCtx.Observer);
            await TaskDone.Done;
        }
    }
}

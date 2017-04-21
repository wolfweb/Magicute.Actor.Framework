using System.Threading.Tasks;
using Magicube.Actor.GrainInterfaces;
using Magicube.Actor.Implementations.Attributes;
using Magicube.Actor.Implementations.Interfaces;
using Orleans;

namespace Magicube.Actor.Implementations.Impls {
    [Command("job-manage-start")]
    public class JobRunCommand : IAskCommand {
        public async Task Run(GrainContext arg) {
            var commandCtx = arg.CommandContext as ClientCommandContext;
            commandCtx?.Observer.ExecuteCmd(new ClientCommandContext {
                Name = "StartJob",
                ArguementCtx = arg.CommandContext.ArguementCtx
            });
            await TaskDone.Done;
        }
    }
}

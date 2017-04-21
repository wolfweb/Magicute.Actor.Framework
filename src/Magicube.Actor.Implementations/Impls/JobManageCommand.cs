using System.Threading.Tasks;
using Magicube.Actor.Implementations.Attributes;
using Magicube.Actor.Implementations.Interfaces;

namespace Magicube.Actor.Implementations.Impls {
    [Command("job-manage-getlist")]
    public class JobManageCommand : IAnswerCommand {
        public async Task<T> Run<T>(GrainContext arg) {
            return await Task.FromResult((T)(object)InnerPools.Gets());
        }
    }
}

using System.Threading.Tasks;
using Magicube.Actor.Implementations.Interfaces;

namespace Magicube.Actor.Implementations.Impls {
    public class EmptyAnswerCommand : IAnswerCommand {
        public async Task<T> Run<T>(GrainContext arg) {
            return await Task.FromResult(default(T));
        }
    }
}

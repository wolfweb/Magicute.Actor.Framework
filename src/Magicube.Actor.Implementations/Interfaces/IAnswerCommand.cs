using System.Threading.Tasks;

namespace Magicube.Actor.Implementations.Interfaces {
    public interface IAnswerCommand : ICommand {
        Task<T> Run<T>(GrainContext arg);
    }
}
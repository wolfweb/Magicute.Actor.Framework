using System.Threading.Tasks;

namespace Magicube.Actor.Implementations.Interfaces {
    public interface IAskCommand : ICommand {
        Task Run(GrainContext arg);
    }
}

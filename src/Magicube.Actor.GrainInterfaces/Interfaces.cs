using System.Threading.Tasks;
using Orleans;

namespace Magicube.Actor.GrainInterfaces {
    #region Observer
    public interface IClientObserver : IGrainObserver {
        void ExecuteCmd(CommandContext ctx);
    }

    public interface IJobObserver : IGrainObserver {
        void UnRegisterJob(ClientCommandContext ctx);
        void RegisterJob(ClientCommandContext ctx);
    }
    #endregion

    #region Grain
    public interface ICommandGrain<T> : IGrainWithIntegerKey {
        Task<T> Execute(CommandContext cmd);
    }

    #endregion
}

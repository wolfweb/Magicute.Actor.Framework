using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Magicube.Actor.Domain;
using Orleans;

namespace Magicube.Actor.GrainInterfaces {
    #region Observer
    public interface IConnectObserver : IGrainObserver {
        void ExecuteCmd(CmdContext ctx);
    }

    public interface IManagerObserver : IGrainObserver {
        void StopJob(ConnectContext ctx);
        void UpdateJob(ConnectContext ctx);
    }
    #endregion


    #region Grain
    public interface IConnectGrain : IGrainWithIntegerKey {
        Task<bool> Connect(ConnectRequest request, IConnectObserver subscriber);
        Task DisConnect(string clientId);
        Task ExecuteCmd(string clientId, CmdContext ctx);
        Task Manage(IManagerObserver subscriber);
        Task Ping(string clientId);
        Task Pong(string clientId);
        Task RepairReport(string clientId, string message);
        Task RegisteHub(string address, string hubName);
    }

    public interface IReportGrain : IGrainWithIntegerKey {
        Task NotifyReport(string clientId, ReportConfig config);
        Task BeforeReport(DateTime curTime, int id);
    }

    public interface IManageGrain : IGrainWithIntegerKey {
        Task<List<ConnectContext>> GetClients();
    }

    #endregion
}

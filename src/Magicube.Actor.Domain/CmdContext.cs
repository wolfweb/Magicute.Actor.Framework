using System;

namespace Magicube.Actor.Domain {
    [Serializable]
    public class CmdContext {
        public JobNoticeMsg Message { get; set; }
        public TransferContext ArguementContext { get; set; }
    }

    public enum JobNoticeMsg {
        Ping,
        RegisterSuccess,
        RegisterFaild,
        ExecuteJob,
        ExecuteRepair,
        Pause,
        Resume,
        RegisterReport
    }
}

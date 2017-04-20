using System;
using Newtonsoft.Json;

namespace Magicube.Actor.Domain {
    [Serializable]
    public class ReportConfig {
        public int Id               { get; set; }
        public Guid Guid            { get; set; }
        public string Name          { get; set; }
        public bool StartNow        { get; set; }
        public DateTime StartAt     { get; set; }
        public string Predicates    { get; set; }
        public string DataSource    { get; set; }
        public ClientState Status   { get; set; }
        public string DataDtoType   { get; set; }
        public string TopicalField  { get; set; }
        public DateTime? StartDate  { get; set; }
        [JsonConverter(typeof(JobScheduleJsonConverter))]
        public JobSchedule Schedule { get; set; }
    }

    public enum ClientState {
        Running,
        Pause,
        Stop
    }
}

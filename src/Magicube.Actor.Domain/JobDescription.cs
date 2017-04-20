using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Quartz;

namespace Magicube.Actor.Domain {
    [Serializable]
    public class JobDescription {
        public string JobGroup { get; set; }
        public string JobName { get; set; }
        public string Description { get; set; }
        public JobSchedule Schedule { get; set; }
        public TransferContext JobData { get; set; }
    }
    [Serializable]
    public abstract class JobSchedule : MarshalByRefObject {
        public abstract IntervalUnit IntervalType { get; }
    }

    [Serializable]
    public class NullableSchedule : JobSchedule {
        public override IntervalUnit IntervalType => IntervalUnit.Year;
    }

    [Serializable]
    public class ScheduleDateTime {
        public ScheduleDateTime(int hour, int minute, int second) {
            Hour = hour;
            Minute = minute;
            Second = second;
            Validate();
        }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public int Second { get; set; }

        public int DayOfWeek { get; set; }
        public int Day { get; set; }

        public string ToString(string format) {
            var result = Regex.Replace(format, "d+", Day < 10 ? $"0{Day}" : Day.ToString());
            result = Regex.Replace(result, "h+", Hour < 10 ? $"0{Hour}" : Hour.ToString());
            result = Regex.Replace(result, "s+", Second < 10 ? $"0{Second}" : Second.ToString());
            result = Regex.Replace(result, "m+", Minute < 10 ? $"0{Minute}" : Minute.ToString());
            return result;
        }

        private void Validate() {
            if (Hour < 0 || Hour > 23)
                throw new ArgumentException("Hour must be from 0 to 23");
            if (Minute < 0 || Minute > 59)
                throw new ArgumentException("Minute must be from 0 to 59");
            if (Second < 0 || Second > 59)
                throw new ArgumentException("Second must be from 0 to 59");
        }
    }

    #region daily schedule
    [Serializable]
    public abstract class DailyJobSchedule : JobSchedule {
        public ScheduleDateTime TimeOf { get; set; }
    }

    [Serializable]
    public class DayJobSchedule : DailyJobSchedule {
        public override IntervalUnit IntervalType => IntervalUnit.Day;
    }
    [Serializable]
    public class WeekJobSchedule : DailyJobSchedule {
        public override IntervalUnit IntervalType => IntervalUnit.Week;
    }
    [Serializable]
    public class MonthJobSchedule : DailyJobSchedule {
        public override IntervalUnit IntervalType => IntervalUnit.Month;
    }
    [Serializable]
    public class YearJobSchedule : DailyJobSchedule {
        public override IntervalUnit IntervalType => IntervalUnit.Year;
    }

    #endregion

    #region sample schedule
    [Serializable]
    public abstract class SampleJobSchedule : JobSchedule {
        public long IntervalStep { get; set; }
    }
    [Serializable]
    public class SecondJobSchedule : SampleJobSchedule {
        public override IntervalUnit IntervalType => IntervalUnit.Second;
    }
    [Serializable]
    public class MinuteJobSchedule : SampleJobSchedule {
        public override IntervalUnit IntervalType => IntervalUnit.Minute;
    }
    [Serializable]
    public class HourJobSchedule : SampleJobSchedule {
        public override IntervalUnit IntervalType => IntervalUnit.Hour;
    }
    #endregion

    #region JsonConvert
    public abstract class JsonCreationConverter<T> : JsonConverter {
        protected const String Fields = "IntervalType";
        protected abstract T Create(JObject jObject);

        public override bool CanConvert(Type objectType) {
            return typeof(T).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
            var jstokens = JToken.ReadFrom(reader);

            var jObject = JObject.Parse(jstokens.ToString());
            T target = Create(jObject);
            serializer.Populate(jObject.CreateReader(), target);
            return target;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
            serializer.DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate;
            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.Serialize(writer, value);
        }
    }

    public class JobScheduleJsonConverter : JsonCreationConverter<JobSchedule> {
        static readonly ConcurrentDictionary<Type, PropertyInfo[]> Properties = new ConcurrentDictionary<Type, PropertyInfo[]>();
        protected override JobSchedule Create(JObject jObject) {
            JobSchedule result = new NullableSchedule();
            var v = jObject[Fields];
            if (v == null) return result;
            var val = (IntervalUnit)Int32.Parse(v.ToString());
            switch (val) {
                case IntervalUnit.Second:
                    result = new SecondJobSchedule();
                    break;
                case IntervalUnit.Minute:
                    result = new MinuteJobSchedule();
                    break;
                case IntervalUnit.Hour:
                    result = new HourJobSchedule();
                    break;
                case IntervalUnit.Day:
                    result = new DayJobSchedule();
                    break;
                case IntervalUnit.Week:
                    result = new WeekJobSchedule();
                    break;
                case IntervalUnit.Month:
                    result = new MonthJobSchedule();
                    break;
            }

            var properties = Properties.GetOrAdd(result.GetType(), type => type.GetProperties());
            foreach (var item in jObject) {
                if (item.Key != Fields) {
                    var property = properties.FirstOrDefault(m => m.Name == item.Key);
                    if (property != null && property.CanWrite) {
                        property.SetValue(result, item.Value.ToObject(property.PropertyType));
                    }
                }
            }
            return result;
        }
    }

    #endregion
}
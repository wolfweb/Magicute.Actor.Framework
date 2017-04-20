using System;
using System.Configuration;

namespace Magicube.Actor.SchedulerClient.Configurations {
    public class ConfigManagerHelper {
        public static T Get<T>(string key, T defValue) {
            var value = ConfigurationManager.AppSettings.Get(key);
            if (!string.IsNullOrEmpty(value)) {
                var theType = typeof(T);
                return (T)Convert.ChangeType(value, theType);
            }
            return defValue;
        }
    }
}

using System.Collections.Concurrent;
using System.Configuration;
using System.Linq;
using Magicube.Actor.Domain;
using Magicube.Actor.SchedulerClient.Configurations;

namespace Magicube.Actor.SchedulerClient.Security {
    public interface IAuthenticationService {
        bool SignIn(SignInRequest request);
    }

    public class AuthenticationService : IAuthenticationService {
        private readonly ConcurrentDictionary<ManageTerminal, ClientIp[]> _dict;

        public AuthenticationService(string sectionName) {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
            var section = (ManageConfigurationSection)config.GetSection(sectionName);
            _dict = new ConcurrentDictionary<ManageTerminal, ClientIp[]>();
            var account = new ManageTerminal {
                Account = section.Manage.Account,
                Password = section.Manage.Password
            };
            _dict.TryAdd(account, (from ManageClientElement it in section.Manage select new ClientIp { Host = it.Host }).ToArray());
        }

        public bool SignIn(SignInRequest request) {
            if (!_dict.Keys.Any(m => m.Account == request.Account && m.Password == request.Password))
                return false;

            if (!_dict.Values.Any(m => m.All(it => it.Host != request.Host)))
                return false;

            return true;
        }
    }
}

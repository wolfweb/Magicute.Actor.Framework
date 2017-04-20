using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Magicube.Actor.Domain {
    public class SignInRequest {
        public string Account { get; set; }
        public string Password { get; set; }
        public string Host { get; set; }
    }
}

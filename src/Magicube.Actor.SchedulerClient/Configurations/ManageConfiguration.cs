using System.Configuration;

namespace Magicube.Actor.SchedulerClient.Configurations {
    public class ManageConfigurationSection : ConfigurationSection {
        [ConfigurationProperty("name", DefaultValue = "Manager", IsRequired = true)]
        public string Name { get { return (string)this["name"]; } set { this["name"] = value; } }

        [ConfigurationProperty("clients")]
        public ManageClientCollection Manage
        {
            get
            {
                ManageClientCollection urlsCollection = (ManageClientCollection)base["clients"];
                return urlsCollection;
            }
        }
    }

    public class ManageClientCollection : ConfigurationElementCollection {
        [ConfigurationProperty("account", DefaultValue = "Admin", IsRequired = true)]
        public string Account { get { return (string)this["account"]; } set { this["account"] = value; } }

        [ConfigurationProperty("password", DefaultValue = "", IsRequired = true)]
        public string Password { get { return (string)this["password"]; } set { this["password"] = value; } }

        public ConfigurationElement this[int index]
        {
            get
            {
                return BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null) {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }


        protected override ConfigurationElement CreateNewElement() {
            return new ManageClientElement();
        }

        protected override object GetElementKey(ConfigurationElement element) {
            return (ManageClientElement)element;
        }
    }

    public class ManageClientElement : ConfigurationElement {
        [ConfigurationProperty("host", DefaultValue = "192.168.0.171", IsRequired = true)]
        public string Host { get { return (string)this["host"]; } set { this["host"] = value; } }
    }
}

using System;

namespace Magicube.Actor.Implementations.Attributes {
    public class CommandAttribute : Attribute {
        public string Name { get; set; }
        public CommandAttribute(string name) {
            Name = name;
        }
    }
}

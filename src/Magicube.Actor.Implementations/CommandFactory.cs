using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using Magicube.Actor.Implementations.Attributes;
using Magicube.Actor.Implementations.Impls;
using Magicube.Actor.Implementations.Interfaces;

namespace Magicube.Actor.Implementations {
    public class CommandFactory {
        private static readonly ConcurrentDictionary<string, CommandDescriptor> Commands = new ConcurrentDictionary<string, CommandDescriptor>();
        public CommandFactory() {
            Initialize();
        }

        public async Task<T> Execute<T>(GrainContext arg) {
            CommandDescriptor command;
            if (Commands.TryGetValue(arg.CommandContext.Name, out command)) {
                if (command.AskCommand != null)
                    await command.AskCommand.Run(arg);
                return await command.AnswerCommand.Run<T>(arg);
            }
            Console.WriteLine($"can not find command with name {arg.CommandContext.Name}!!!");
            return await new EmptyAnswerCommand().Run<T>(arg);
        }

        private void Initialize() {
            var commands = typeof(CommandFactory).Assembly.GetTypes()
                            .Where(m => !m.IsInterface && !m.IsAbstract)
                            .Where(m => typeof(ICommand).IsAssignableFrom(m)).ToArray();

            foreach (var command in commands) {
                var attr = command.GetCustomAttributes(typeof(CommandAttribute), false).FirstOrDefault() as CommandAttribute;
                if (attr == null) {
                    Console.WriteLine($"command of {command.FullName} is not assigned commandattribute!!!");
                    continue;
                }
                var desc = new CommandDescriptor {
                    AnswerCommand = new EmptyAnswerCommand()
                };

                if (typeof(IAskCommand).IsAssignableFrom(command)) {
                    desc.AskCommand = (IAskCommand)Activator.CreateInstance(command);
                    Commands.AddOrUpdate(attr.Name, desc, (k, v) => {
                        v.AskCommand = (IAskCommand)Activator.CreateInstance(command);
                        return v;
                    });
                } else {
                    desc.AnswerCommand = (IAnswerCommand)Activator.CreateInstance(command);
                    Commands.AddOrUpdate(attr.Name, desc, (k, v) => {
                        v.AnswerCommand = (IAnswerCommand)Activator.CreateInstance(command);
                        return v;
                    });
                }
            }
        }

        sealed class CommandDescriptor {
            public IAskCommand AskCommand { get; set; }
            public IAnswerCommand AnswerCommand { get; set; }
        }
    }
}

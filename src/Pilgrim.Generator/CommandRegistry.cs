using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pilgrim.Generator
{
    public struct CommandRegistry
    {
        string command;
        ICommandOptions parser;

        public string Command { get { return command; } }
        public ICommandOptions Parser { get { return parser; } }

        public CommandRegistry(string command, ICommandOptions parser)
        {
            this.command = command;
            this.parser = parser;
        }
    }
}

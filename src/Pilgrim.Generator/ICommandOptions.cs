using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using Simple.Patterns;
using Pilgrim.Generator.Parsers;

namespace Pilgrim.Generator
{
    public interface ICommandOptions
    {
        ICommandParser Argument { get; }
        IEnumerable<ICommandParser> Options { get; }
        string GeneratorType { get; }
        ICommand Parse(string parameters, bool ignoreExceedingArgs);
    }
}

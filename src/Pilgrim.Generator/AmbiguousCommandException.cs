﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple;

namespace Pilgrim.Generator
{
    [Serializable]
    public class AmbiguousCommandException : ParserException
    {
        public AmbiguousCommandException() { }
        public AmbiguousCommandException(string command, IEnumerable<ICommandOptions> generators)
            : base("Multiple commands found for input '{0}': {1}".AsFormatFor(
                command, GetParserListString(generators))) { }

        private static string GetParserListString(IEnumerable<ICommandOptions> parsers)
        {
            return string.Join(", ", parsers.Select(x => x.GeneratorType).ToArray());
        }

        protected AmbiguousCommandException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

}

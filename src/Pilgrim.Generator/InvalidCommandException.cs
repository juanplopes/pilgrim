﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple;

namespace Pilgrim.Generator
{
    [Serializable]
    public class InvalidCommandException : ParserException
    {
        public InvalidCommandException() { }
        public InvalidCommandException(string command)
            : base("No command found for input '{0}'".AsFormatFor(command)) { }

        protected InvalidCommandException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }

}

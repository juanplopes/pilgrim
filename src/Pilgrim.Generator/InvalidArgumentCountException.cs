﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple;

namespace Pilgrim.Generator
{
    [Serializable]
    public class InvalidArgumentCountException : ParserException
    {
        public InvalidArgumentCountException() { }
        public InvalidArgumentCountException(string command, int expected, int actual)
            : base("Invalid argument count in '{0}'. Expected: {1}. Found: {2}".AsFormatFor(
                command, expected, actual)) { }


        protected InvalidArgumentCountException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}

﻿using System;
using System.Collections.Generic;
using Simple.Patterns;
namespace Pilgrim.Generator.HelpWriter
{
    public interface IHelpWriter
    {
        void Write(IEnumerable<CommandRegistry> commands);
        void Write(CommandResolver resolver);
    }
}

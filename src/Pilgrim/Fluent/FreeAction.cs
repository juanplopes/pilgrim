﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pilgrim.Framework;

namespace Pilgrim
{
    public class FreeAction : IAction
    {
        public Action<ITransformationProvider> Action { get; set; }

        public FreeAction(Action<ITransformationProvider> action)
        {
            this.Action = action;
        }

        public void Execute(ITransformationProvider provider)
        {
            Action(provider);
        }
    }
}

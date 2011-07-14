using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pilgrim.Framework;

namespace Pilgrim
{
    public interface IAction
    {
        void Execute(ITransformationProvider provider);
    }
}

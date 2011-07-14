﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pilgrim.Framework;

namespace Pilgrim
{
    public class TableRemoveAction : TableAction
    {
        public TableRemoveAction(SchemaAction database, string name) : base(database, name) { }

        public override void Execute(ITransformationProvider provider)
        {
            provider.RemoveTable(this.Name);
            base.Execute(provider);
        }
    }
}

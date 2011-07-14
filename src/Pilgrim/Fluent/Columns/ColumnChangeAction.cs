﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Pilgrim.Framework;

namespace Pilgrim
{
    public class ColumnChangeAction : ColumnAction
    {
        public ColumnChangeAction(TableAction table, string name, DbType type) : base(table, name, type) { }

        public override void Execute(ITransformationProvider provider)
        {
            provider.ChangeColumn(Table.Name, this.ToColumn());
        }
    }
}

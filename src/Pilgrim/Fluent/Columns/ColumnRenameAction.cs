﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pilgrim.Framework;

namespace Pilgrim
{
    public class ColumnRenameAction : ColumnNameAction
    {
        public string FromName { get; set; }

        public ColumnRenameAction(TableAction table, string fromName, string toName) 
            : base(table, toName)
        {
            this.FromName = fromName;
        }

        public override void Execute(ITransformationProvider migration)
        {
            migration.RenameColumn(Table.Name, FromName, Name);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Pilgrim.Framework;

namespace Pilgrim
{
    public class ColumnAddAction : ColumnAction
    {
        public ColumnAddAction(TableAction table, string name, DbType type) : base(table, name, type) { }
        public override void Execute(ITransformationProvider provider)
        {
            provider.AddColumn(Table.Name, this.ToColumn());
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pilgrim.Framework;

namespace Pilgrim
{
    public class UniqueConstraintAddAction : UniqueConstraintAction
    {
        public IList<ColumnNameAction> Columns { get; set; }

        public UniqueConstraintAddAction(TableAction table, string name, IList<ColumnNameAction> columns)
            : base(table, name)
        {
            Name = name;
            Columns = columns;
        }

        public override void Execute(ITransformationProvider provider)
        {
            provider.AddUniqueConstraint(Name, Table.Name,
                Columns.Select(x => x.Name).ToArray());
        }

    }
    
}

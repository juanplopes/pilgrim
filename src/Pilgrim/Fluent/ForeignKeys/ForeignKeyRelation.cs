using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pilgrim
{
    public class ForeignKeyRelation
    {
        public ColumnNameAction FromColumn { get; set; }
        public string ToColumn { get; set; }

        public ForeignKeyRelation(ColumnNameAction fromColumn, string toColumn)
        {
            FromColumn = fromColumn;
            ToColumn = toColumn;
        }

        public static implicit operator ForeignKeyRelation(ColumnNameAction column)
        {
            return column.LinkedTo(column.Name);
        }
    }
}

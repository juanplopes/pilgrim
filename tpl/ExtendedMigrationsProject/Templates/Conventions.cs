using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Simple;
using Pilgrim.Generator;
using Pilgrim.Generator.Strings;
using Pilgrim.Generator.Metadata;

namespace $safeprojectname$
{
    public class Conventions : ITableConventions
    {
        public string NameFor(DbTableName table)
        {
            return table.Name.CleanUp();
        }

        public string TypeFor(DbColumn column)
        {
            return column.GetDisplayTypeName(false);
        }

        public string NameFor(DbColumn column)
        {
            return column.Name.CleanUp();
        }

        public string TypeFor(DbManyToOne fk)
        {
            return NameFor(fk.PkTableRef);
        }

        public string NameFor(DbManyToOne fk)
        {
            if (fk.Columns.Count == 1)
                return fk.Columns[0].FkColumnRef.Name.ReplaceId().CleanUp();
            else
                return NameFor(fk.PkTableRef);
        }

        public string TypeFor(DbOneToMany fk)
        {
            return "ICollection<" + NameFor(fk.FkTableRef) + ">";
        }

        public string ConcreteTypeFor(DbOneToMany fk)
        {
            return "HashSet<" + NameFor(fk.FkTableRef) + ">";
        }


        public string NameFor(DbOneToMany fk)
        {
            var name = NameFor(fk.FkTableRef) + "List";
            if (!fk.SafeNaming)
                name += "At" + fk.Columns[0].Name.ReplaceId().CleanUp();
            return name;
        }

    }
}

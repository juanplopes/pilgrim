using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pilgrim.Generator.Metadata;

namespace Pilgrim.Generator
{
    public interface ITableConventions
    {
        string NameFor(DbOneToMany fk);
        string NameFor(DbManyToOne fk);
        string NameFor(DbTableName table);
        string NameFor(DbColumn column);
        string TypeFor(DbManyToOne fk);
        string TypeFor(DbColumn column);
    }
}

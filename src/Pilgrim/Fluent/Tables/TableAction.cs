﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pilgrim.Framework;
using System.Data;

namespace Pilgrim
{


    public abstract partial class TableAction : IAction
    {
        public SchemaAction Database { get; set; }
        public string Name { get; set; }
        public IList<IAction> Actions { get; set; }

        public TableAction(SchemaAction database, string name)
        {
            Name = name;
            Database = database;
            Actions = new List<IAction>();
        }

        public ColumnAddAction AddColumn(string name, DbType type)
        {
            var column = new ColumnAddAction(this, name, type);
            Actions.Add(column);
            return column;
        }

        public UniqueConstraintAddAction UniqueColumns(string name, params ColumnAction[] columns)
        {
            var action = new UniqueConstraintAddAction(this, name, columns);
            Actions.Add(action);
            return action;
        }

        public ForeignKeyAddAction AutoForeignKey(string primaryKeyTable, params ForeignKeyRelation[] columns)
        {
            return AutoForeignKey(primaryKeyTable, null, columns);
        }

        public ForeignKeyAddAction AutoForeignKey(string primaryKeyTable, string tag, params ForeignKeyRelation[] columns)
        {
            return ForeignKey(
                Database.Convention.ForeignKeyConstraint(Name, string.Empty, primaryKeyTable, string.Empty, tag),
                primaryKeyTable, columns);
        }
        public ForeignKeyAddAction ForeignKey(string name, string primaryKeyTable, params ForeignKeyRelation[] columns)
        {
            var action = new ForeignKeyAddAction(this, name, columns.Select(x => x.FromColumn.Name).ToArray(),
                   primaryKeyTable, columns.Select(x => x.ToColumn).ToArray());
            Actions.Add(action);
            return action;
        }

        public ColumnNameAction ColumnName(string name)
        {
            return new ColumnNameAction.Concrete(this, name);
        }
      
        public virtual void Execute(ITransformationProvider provider)
        {
            foreach (var action in this.Actions)
            {
                action.Execute(provider);
            }
        }

        public IndexAddAction AddIndex(string name, params string[] columns)
        {
            var action = new IndexAddAction(this, Database.Convention.IndexKey(this.Name, name), columns);
            Actions.Add(action);
            return action;
        }

        public IndexRemoveAction RemoveIndex(string name)
        {
            var action = new IndexRemoveAction(this, Database.Convention.IndexKey(this.Name, name));
            Actions.Add(action);
            return action;
        }
    }
}

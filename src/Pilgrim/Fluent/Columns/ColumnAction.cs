﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pilgrim.Framework;
using System.Data;

namespace Pilgrim
{
    public abstract class ColumnAction : ColumnNameAction
    {
        public DbType Type { get; set; }
        public int? Size { get; set; }
        public object DefaultValue { get; set; }
        public ColumnProperty Properties { get; set; }

        public Column ToColumn()
        {
            var column = new Column(Name, Type);
            column.ColumnProperty = Properties;

            if (Size != null) column.Size = Size.Value;
            if (DefaultValue != null) column.DefaultValue = DefaultValue;
            return column;
        }

        public ColumnAction(TableAction table, string name) : base(table, name)
        {
        }

        public ColumnAction(TableAction table, string name, DbType type)
            : this(table, name)
        {
            Type = type;
        }

        public ColumnAction WithSize(int size)
        {
            Size = size;
            return this;
        }

        public ColumnAction ToType(DbType type)
        {
            Type = type;
            return this;
        }

        public ColumnAction Default(object value)
        {
            DefaultValue = value;
            return this;
        }

        public ColumnAction WithProperties(params ColumnProperty[] properties)
        {
            foreach (var property in properties)
                Properties = Properties | property;

            return this;
        }

        public ColumnAction PrimaryKeyWithIdentity()
        {
            return WithProperties(ColumnProperty.PrimaryKeyWithIdentity);
        }

        public ColumnAction PrimaryKey()
        {
            return WithProperties(ColumnProperty.PrimaryKey);
        }

        public ForeignKeyAddAction AutoForeignKey(string primaryKeyTable)
        {
            return AutoForeignKey(primaryKeyTable, null);
        }

        public ForeignKeyAddAction AutoForeignKey(string primaryKeyTable, string tag)
        {
            return ForeignKey(
                Table.Database.Convention.ForeignKeyConstraint(Table.Name, string.Empty, primaryKeyTable, string.Empty, tag), 
                primaryKeyTable);
        }

        public ForeignKeyAddAction ForeignKey(string name, string primaryKeyTable)
        {
            return ForeignKey(name, primaryKeyTable, Table.Database.Convention.PrimaryKeyColumn(primaryKeyTable));
        }


        public ForeignKeyAddAction ForeignKey(string name, string primaryKeyTable, string primaryKeyColumn)
        {
            WithProperties(ColumnProperty.ForeignKey);
            return Table.AddForeignKey(name, primaryKeyTable, this.LinkedTo(primaryKeyColumn));
        }

        public override ForeignKeyRelation LinkedTo(string column)
        {
            this.WithProperties(ColumnProperty.ForeignKey);
            return base.LinkedTo(column);
        }

        public UniqueConstraintAddAction Unique(string constraintName)
        {
            return Table.AddUniqueColumns(constraintName, this);
        }

        public ColumnAction Nullable()
        {
            return WithProperties(ColumnProperty.Null);
        }

        public ColumnAction NotNullable()
        {
            return WithProperties(ColumnProperty.NotNull);
        }

        public ColumnAction Indexed()
        {
            return WithProperties(ColumnProperty.Indexed);
        }

        public ColumnAction Unsigned()
        {
            return WithProperties(ColumnProperty.Unsigned);
        }
    }
}

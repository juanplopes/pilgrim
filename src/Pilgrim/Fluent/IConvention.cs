﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pilgrim
{
    public interface IConvention
    {
        string PrimaryKeyColumn(string table);
        string ForeignKeyConstraint(string fkTable, string fkColumn, string pkTable, string pkColumn, string tag);
        string IndexKey(string table, string name);
    }

    public class DefaultConvention : IConvention
    {
        #region IConvention Members

        public virtual string PrimaryKeyColumn(string table)
        {
            return "id";
        }
        
        public virtual string ForeignKeyConstraint(string fkTable, string fkColumn, string pkTable, string pkColumn, string tag)
        {
            string res = fkTable + "_" + pkTable;
            if (!string.IsNullOrEmpty(tag)) res += "_" + tag;
            return res + "_fk";
        }

        public virtual string IndexKey(string table, string name)
        {
            return string.Format("IDX_{0}_{1}", table, name);
        }

        #endregion
    }
}

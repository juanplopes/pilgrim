﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pilgrim.Framework;

namespace Pilgrim
{
    public class TableRenameAction : IAction
    {
        public string FromName { get; set; }
        public string ToName { get; set; }

        public TableRenameAction(string fromName, string toName)
        {
            this.ToName = toName;
            this.FromName = fromName;
        }

        #region IAction Members

        public void Execute(ITransformationProvider provider)
        {
            provider.RenameTable(FromName, ToName);
        }
        #endregion
    }
}

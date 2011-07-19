using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pilgrim.Generator.Metadata;

namespace Pilgrim.Generator
{
    public interface ITableTemplate
    {
        void Create(DbTable table);
        ITableTemplate WithConventions(ITableConventions conventions);
        ITableTemplate As(string type);
        ITableTemplate Target(string project, string file);
    }
}

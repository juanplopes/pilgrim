using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pilgrim;
using System.Reflection;

namespace $safeprojectname$
{
    class Program
    {
        static void Main(string[] args)
        {
            var migrator = new DbMigrator(
                new MigratorOptions("ConnectionStringName").FromAssembly(Assembly.GetExecutingAssembly()));
            migrator.MigrateToLast();
        }
    }
}

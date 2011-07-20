using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pilgrim.Generator;
using System.IO;
using Pilgrim;
using System.Reflection;

namespace $safeprojectname$.Templates
{
    public class MigrateTool : ICommand
    {
        public long? Version { get; set; }
        public string FilePath { get; set; }
        public string Environment { get; set; }

        public MigrateTool(string environment)
        {
            this.Environment = environment;
        }

        public void Execute()
        {
            var builder = new StringBuilder();
            Action<string> action = null;
            
            if (FilePath != null)
                action = x => builder.AppendLine(x);

            new DbMigrator(new MigratorOptions(Environment)
                .WriteWith(action)
                .FromAssembly(Assembly.GetExecutingAssembly()))
                .Migrate(Version);

            if (FilePath != null)
                File.WriteAllText(FilePath, builder.ToString());
        }
    }
}

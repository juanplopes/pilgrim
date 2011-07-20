using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pilgrim.Generator;
using Pilgrim.Generator.Metadata;
using System.IO;
using System.Resources;
using System.Reflection;

namespace $safeprojectname$.Templates
{
    public class ScaffoldGenerator : ICommand
    {
        public string Table { get; set; }
        public string Environment { get; set; }
        public string BaseDir { get; set; }

        public ScaffoldGenerator(string environment)
        {
            this.Environment = environment;
            this.BaseDir = "";
        }

        public void Execute()
        {
            var table = new DbSchema(Environment).GetTables(Table).First();
            Create(table, "SampleTemplate.txt", "$safeprojectname$.csproj", "{0}.cs");
        }

        #region Generator
        public void Create(DbTable dbTable, string template, string project, string file)
        {
            var resName = string.Format("$safeprojectname$.Templates.{0}", template);
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resName))
            {
                if (!Path.IsPathRooted(project))
                    project = Path.Combine(BaseDir, project);
                var conventions = new Conventions();
                var tpl = new TableTemplate(new StreamReader(stream).ReadToEnd());
                tpl.SetMany(re => conventions, table => dbTable);
                tpl.WithConventions(conventions).Target(project, file).Create(dbTable);
            }
        }
        #endregion
    }
}

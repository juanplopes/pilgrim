using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pilgrim;
using System.Reflection;
using $safeprojectname$.Templates;
using Pilgrim.Generator;

namespace $safeprojectname$
{
    class Program
    {
        static void Main(string[] args)
        {
            var runner = new ConsoleResolver();

            runner.Register(() => new MigrateTool("Pilgrim"), "migrate")
               .WithOption("to", x => x.Version)
               .WithOption("save", x => x.FilePath);

            runner.Register(() => new ScaffoldGenerator("Pilgrim"), "scaffold")
                .WithArgument("table", x => x.Table)
                .WithOption("to", x => x.BaseDir);

            runner.WithHelp();

            runner.Run(string.Join(" ", args));
        }
    }
}

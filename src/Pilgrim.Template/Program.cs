using System;
using System.Collections.Generic;
using System.Reflection;
using System.GAC;

namespace Pilgrim.Template
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var cmd = args.Length > 0 ? args[0] : "install";
            var cache = AssemblyCache.CreateAssemblyCache();
            var location = Assembly.GetExecutingAssembly().Location;
            var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

            uint res = 0;
            Console.WriteLine(cache.UninstallAssembly(2, assemblyName, null, out res));
            Console.WriteLine((IASSEMBLYCACHE_UNINSTALL_DISPOSITION)res);

            if (cmd == "install") 
                Console.WriteLine(cache.InstallAssembly((uint)IASSEMBLYCACHE_INSTALL_FLAG.IASSEMBLYCACHE_INSTALL_FLAG_FORCE_REFRESH, location, null));
            return 0;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pilgrim.Generator
{
    public class ConsoleResolver : CommandResolver
    {
        public void Run(string cmd)
        {
            try
            {
                Resolve(cmd).Execute();
            }
            catch (ParserException e)
            {
                System.Console.Error.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                while (e != null)
                {
                    System.Console.Error.WriteLine(e.Message);
                    System.Console.Error.WriteLine(e.StackTrace);
                    e = e.InnerException;
                }
            }
        }
    }
}

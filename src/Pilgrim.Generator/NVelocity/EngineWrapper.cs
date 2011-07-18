using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NVelocity.App;
using Simple.Patterns;

namespace Simple.NVelocity
{
    public class EngineWrapper
    {
        public static EngineWrapper Instance
        {
            get { return Nested.instance; }
        }

        class Nested
        {
            static Nested() { }

            internal static readonly EngineWrapper instance = new EngineWrapper();
        }

        VelocityEngine _engine = null;
        EngineWrapper()
        {
            _engine = new VelocityEngine();
            _engine.Init();
        }

        public VelocityEngine Get()
        {
            return _engine;
        }
    }
}

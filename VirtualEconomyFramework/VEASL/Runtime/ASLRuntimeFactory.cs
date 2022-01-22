using System;
using System.Collections.Generic;
using System.Text;

namespace VEASL.Runtime
{
    public static class ASLRuntimeFactory
    {
        public static IASLRuntime GetASLRuntime(ASLRuntimeType type)
        {
            IASLRuntime runtime = null;
            switch (type)
            {
                case ASLRuntimeType.Main:
                    runtime = new ASLRuntime();
                    break;
            }
            return runtime;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ASL.Functions
{
    public class FunctionBase
    {
        public int Location { get; set; } = 0;
        public bool Done { get; set; } = false;
        public bool Success { get; set; } = false;
        public bool Failed { get; set; } = false;
    }
    public class Function : FunctionBase
    {
        public Opcodes OpCode { get; set; } = Opcodes.none;
        public string[] Paramseters { get; set; }

        public Func<string, string[], Task<List<object>>> Func { get; set; } = null;
    }
}

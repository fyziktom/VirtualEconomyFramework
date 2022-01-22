using System;
using System.Collections.Generic;
using System.Text;

namespace ASL.Variables
{
    public class Variable
    {
        public string Name { get; set; } = string.Empty;
        public object Value { get; set; } = null;
        public int Scope { get; set; } = 1;

        public Variable(string name)
        {
            Name = name;
            Value = null;
        }
    }
}

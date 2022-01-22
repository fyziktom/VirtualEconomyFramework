using System;
using System.Collections.Generic;
using System.Text;

namespace ASL.Variables
{
    /// <summary>
    /// This is observed Variable - this variable is parsed and tracked from the NFTs
    /// </summary>
    public class OVariable
    {
        private static object _lock = new object();
        public string Name { get; set; } = string.Empty;
        public Type VarType { get; set; } = typeof(string);
        public string Represents { get; set; } = string.Empty;
        private List<object> _values = new List<object>();
        public List<object> Values
        {
            get => _values;
        }
        public int Scope { get; set; } = 1;

        public OVariable(string name)
        {
            Name = name;
            _values = new List<object>();
        }
        public void AddNewValues(List<object> values)
        {
            if (values != null)
                lock (_lock)
                {
                    values.ForEach(v => _values.Add(v));
                }
        }
    }
}

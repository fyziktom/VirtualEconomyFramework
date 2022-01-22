using System;
using System.Collections.Generic;
using System.Text;

namespace ASL.Variables
{
    public enum ResultType
    {
        One,
        Dict
    }
    public class Result
    {
        public ResultType Type { get; set; } = ResultType.Dict;
        public List<object> ResultList { get; set; } = new List<object>();
    }
}

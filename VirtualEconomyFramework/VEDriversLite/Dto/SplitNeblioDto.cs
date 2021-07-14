using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Dto
{
    public class SplitNeblioDto
    {
        public List<string> receivers { get; set; } = new List<string>();
        public int lots { get; set; } = 2;
        public double amount { get; set; } = 0.05;

        public double TotalAmount => amount * lots;
    }
}

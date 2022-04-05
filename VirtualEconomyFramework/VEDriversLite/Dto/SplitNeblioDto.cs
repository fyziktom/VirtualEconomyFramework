using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Dto
{
    /// <summary>
    /// Dto for split Neblio transaction request
    /// </summary>
    public class SplitNeblioDto
    {
        /// <summary>
        /// Addresses of the receivers
        /// </summary>
        public List<string> receivers { get; set; } = new List<string>();
        /// <summary>
        /// Number of the lots
        /// </summary>
        public int lots { get; set; } = 2;
        /// <summary>
        /// Amount of the Neblio in one lot
        /// </summary>
        public double amount { get; set; } = 0.05;

        /// <summary>
        /// Total amount of all lots together
        /// </summary>
        public double TotalAmount => amount * lots;
    }
}

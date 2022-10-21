using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.PVECalculations;

namespace VEDriversLite.EntitiesBlocks.StorageCalculations
{
    /// <summary>
    /// One Battery block in Battery Block Handler.
    /// It represents one battery or block of batteries connected together and act as one endpoint.
    /// </summary>
    public class BatteryBlock
    {
        /// <summary>
        /// Id of battery block
        /// </summary>
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Group Id where this Battery Block belongs
        /// </summary>
        public string GroupId { get; set; } = string.Empty;
        /// <summary>
        /// Name of battery block
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Capacity of battery block.
        /// For example 20000 Wh
        /// </summary>
        public double Capacity { get; set; } = 0.0;
        /// <summary>
        /// Actual filled capacity of the battery block.
        /// Fo example 1000 Wh
        /// </summary>
        public double ActualFilledCapacity { get; set; } = 0.1;
        /// <summary>
        /// Maximum charging power in Wh
        /// </summary>
        public double MaximumChargePower { get; set; } = 1;
        /// <summary>
        /// Maximum discharge power in Wh
        /// </summary>
        public double MaximumDischargePower { get; set; } = 1;
        /// <summary>
        /// Internal resistance - not used in calculation now
        /// </summary>
        public double InternalResistance { get; set; } = 0.0;



        /// <summary>
        /// Fill object with source
        /// </summary>
        /// <param name="panel"></param>
        public void Fill(BatteryBlock panel)
        {
            foreach (var param in typeof(BatteryBlock).GetProperties())
            {
                try
                {
                    if (param.CanWrite)
                    {
                        var value = param.GetValue(panel);
                        var paramname = param.Name;
                        var pr = typeof(BatteryBlock).GetProperties().FirstOrDefault(p => p.Name == paramname);
                        if (pr != null)
                            pr.SetValue(this, value);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Cannot load parameter." + ex.Message);
                }
            }
        }

        /// <summary>
        /// Clone this
        /// </summary>
        public BatteryBlock Clone()
        {
            var res = new BatteryBlock();
            foreach (var param in typeof(BatteryBlock).GetProperties())
            {
                try
                {
                    if (param.CanWrite)
                    {
                        var value = param.GetValue(this);
                        var paramname = param.Name;
                        var pr = typeof(BatteryBlock).GetProperties().FirstOrDefault(p => p.Name == paramname);
                        if (pr != null)
                            pr.SetValue(res, value);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Cannot load parameter. " + ex.Message);
                }
            }
            return res;
        }
    }
}

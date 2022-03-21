using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.NFT.Imaging.Xray.Dto
{
    /// <summary>
    /// Type of the Xray tube anode.
    /// Most of the Xray tubes uses Reflexive or Transmision
    /// </summary>
    public enum XrayTubeAnodeType
    {
        Reflexive,
        Transmision,
        ReflexiveRotary,
        Straton
    }

    /// <summary>
    /// Materials list for primary filtration
    /// </summary>
    public enum XrayPrimaryFiltrationMaterials
    {
        Al,
        Cu,
        Pb,
    }
    public class XrayPrimaryFilter
    {
        /// <summary>
        /// Material of the primary filter
        /// </summary>
        public XrayPrimaryFiltrationMaterials Material { get; set; } = XrayPrimaryFiltrationMaterials.Al;
        /// <summary>
        /// Thickness of the primary filter
        /// </summary>
        public double Thickness { get; set; } = 0.0;
    }
}

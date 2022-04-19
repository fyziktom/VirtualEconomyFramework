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
        /// <summary>
        /// Reflexive anode
        /// </summary>
        Reflexive,
        /// <summary>
        /// Transmision anode
        /// </summary>
        Transmision,
        /// <summary>
        /// Rotary anode with classic reflexive target
        /// </summary>
        ReflexiveRotary,
        /// <summary>
        /// Rotary anode type Straton 
        /// </summary>
        Straton
    }

    /// <summary>
    /// Materials list for primary filtration
    /// </summary>
    public enum XrayPrimaryFiltrationMaterials
    {
        /// <summary>
        /// Aluminium
        /// </summary>
        Al,
        /// <summary>
        /// Cuprum
        /// </summary>
        Cu,
        /// <summary>
        /// Lead
        /// </summary>
        Pb,
    }
    /// <summary>
    /// Primary filter on the xray tube
    /// </summary>
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

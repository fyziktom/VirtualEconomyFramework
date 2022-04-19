using System;
using System.Collections.Generic;
using System.Text;
using VEDriversLite.NFT.Imaging.Xray;

namespace VEDriversLite.NFT.Imaging.Xray.Dto
{
    /// <summary>
    /// Parameters of the Xray detector
    /// </summary>
    public class DetectorDataDto
    {
        /// <summary>
        /// Number of columnts - width
        /// </summary>
        public int W { get; set; } = 0;
        /// <summary>
        /// Number of rows - height
        /// </summary>
        public int H { get; set; } = 0;
        /// <summary>
        /// Bit depth of image data in one pixel
        /// </summary>
        public int Bits { get; set; } = 16;
        /// <summary>
        /// Type of the scintilator
        /// </summary>
        public string Sc { get; set; } = "DRZ";
        /// <summary>
        /// Detection element width in micro meters
        /// </summary>
        public double Pw { get; set; } = 0.0;
        /// <summary>
        /// Detection element height in micro meters
        /// </summary>
        public double Ph { get; set; } = 0.0;
        /// <summary>
        /// Returns true if the all values inside class are in default values
        /// </summary>
        /// <returns></returns>
        public bool IsDefault()
        {
            return (W == 0 && H == 0 && Bits == 16 && Sc == "DRZ" && Pw == 0.0 && Ph == 0.0);
        }
    }
    /// <summary>
    /// Parameters of the Xray source
    /// </summary>
    public class SourceParametersDto
    {
        /// <summary>
        /// X-ray tube anode type.
        /// Most of the Xray tubes uses Reflexive or Transmision
        /// R - Reflexive
        /// T - Transmision
        /// </summary>
        public string AT { get; set; } = "R";
        /// <summary>
        /// Material of the anode target. Usually it is made from Wolfram
        /// </summary>
        public string TM { get; set; } = "W";
        /// <summary>
        /// Focal spot width in micro meters
        /// </summary>
        public double FW { get; set; } = 0.0;
        /// <summary>
        /// Focal spot height in micro meters
        /// </summary>
        public double FH { get; set; } = 0.0;
        /// <summary>
        /// Focal spot angle if Reflexive
        /// </summary>
        public double FA { get; set; } = 0.0;
        /// <summary>
        /// Minimum voltage on anode
        /// </summary>
        public double MinV { get; set; } = 0.0;
        /// <summary>
        /// Maximum voltage on anode
        /// </summary>
        public double MaxV { get; set; } = 0.0;
        /// <summary>
        /// Returns true if the all values inside class are in default values
        /// </summary>
        /// <returns></returns>
        public bool IsDefault()
        {
            return (AT == "R" && TM == "W" && FW == 0.0 && FH == 0.0 && FA == 0.0 && MinV == 0.0 && MaxV == 0.0);
        }
    }
    /// <summary>
    /// Parameters of the axis of positionning system
    /// </summary>
    public class AxisDto
    {
        /// <summary>
        /// Axis name: X, Y, Z, R
        /// R is rotation stand for the object
        /// Robotics Joints - R1, R2, R3, R4, R5, R6
        /// </summary>
        public string Ax { get; set; } = "X";
        /// <summary>
        /// Accuracy of step
        /// </summary>
        public double Ac { get; set; } = 0.0;
        /// <summary>
        /// Length of whole axis
        /// </summary>
        public double L { get; set; } = 0.0;
        /// <summary>
        /// Number of steps per whole axis
        /// </summary>
        public double Stp { get; set; } = 0.0;
        /// <summary>
        /// Start of the used part of the axis - virtual zero point
        /// </summary>
        public double Start { get; set; } = 0.0;
        /// <summary>
        /// End of the used part of the axis - virtual end point
        /// </summary>
        public double End { get; set; } = 0.0;
    }
    /// <summary>
    /// Positioner parameters, including axis parameters
    /// </summary>
    public class PositionerParametersDto
    {
        /// <summary>
        /// Information about the all axes in the positioner
        /// </summary>
        public List<AxisDto> Axes { get; set; } = new List<AxisDto>();
        /// <summary>
        /// Returns true if the all values inside class are in default values
        /// </summary>
        /// <returns></returns>
        public bool IsDefault()
        {
            return (Axes.Count == 0);
        }
    }
    /// <summary>
    /// Scanned object position in the scene
    /// </summary>
    public class ObjectPositionDto
    {
        /// <summary>
        /// Object Position X
        /// </summary>
        public double X { get; set; } = 0.0;
        /// <summary>
        /// Object Position Y
        /// </summary>
        public double Y { get; set; } = 0.0;
        /// <summary>
        /// Object Position Z
        /// </summary>
        public double Z { get; set; } = 0.0;
        /// <summary>
        /// Object Rotation R1
        /// It is rotation in XY plane
        /// Rotation is in degrees
        /// </summary>
        public double R1 { get; set; } = 0.0;
        /// <summary>
        /// Object Rotation R2
        /// It is rotation in XZ plane
        /// Rotation is in degrees
        /// </summary>
        public double R2 { get; set; } = 0.0;
        /// <summary>
        /// Object Rotation R3
        /// It is rotation in ZY plane
        /// Rotation is in degrees
        /// </summary>
        public double R3 { get; set; } = 0.0;
        /// <summary>
        /// Distance between Source Focal spot and Detector
        /// Distance is in milimeters
        /// </summary>
        public double Dsd { get; set; } = 0.0;
        /// <summary>
        /// Distance between Object rotation Axis XY center and Detector
        /// Distance is in milimeters
        /// </summary>
        public double Dod { get; set; } = 0.0;
        /// <summary>
        /// Returns true if the all values inside class are in default values
        /// </summary>
        /// <returns></returns>
        public bool IsDefault()
        {
            return (X == 0.0 && Y == 0.0 && Z == 0.0 && R1 == 0.0 && R2 == 0.0 && R3 == 0.0 && Dsd == 0.0 && Dod == 0.0);
        }

    }
}

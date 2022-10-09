namespace VEDriversLite.EntitiesBlocks.PVECalculations
{
    public static class PVECalcs
    {
        /// <summary>
        /// Get total Peak Power of panel with use of weather factor, sun angle and other params
        /// </summary>
        /// <param name="panelPeakPower">Peak power of panel</param>
        /// <param name="efficiency">efficiency of panel 1 - best efficiency</param>
        /// <param name="weatherFactor">weather factor 1 - best weather</param>
        /// <param name="panelPeakAngle">Sun To Panel angle of Panel Power peak measurement</param>
        /// <param name="sunBeamAngle">Sun angle</param>
        /// <param name="anglesInDegrees">are angles input in degrees?</param>
        /// <param name="round">round result</param>
        /// <param name="decimals">if round, how many decimals, default 4</param>
        /// <returns></returns>
        public static double GetTotalPeakPowerOfPanel(double panelPeakPower, double efficiency, double weatherFactor, double panelPeakAngle, double sunBeamAngle, bool anglesInDegrees = false, bool round = false, int decimals = 4)
        {
            var sunangle = anglesInDegrees ? sunBeamAngle / (180 / Math.PI) : sunBeamAngle;
            var panelpowerangle = anglesInDegrees ? panelPeakAngle / (180 / Math.PI) : panelPeakAngle;
            var res = GetPeakPowerOfPanel(panelPeakPower, efficiency, weatherFactor, sunangle, panelpowerangle);

            var result = res >= 0 ? res : 0;

            return round ? Math.Round(result, decimals) : result;
        }

        /// <summary>
        /// Get Peak power of one panel with use of weather factor, sun angle and other params
        /// </summary>
        /// <param name="panelPeakPower">Peak power of panel</param>
        /// <param name="efficiency">efficiency of panel 1 - best efficiency</param>
        /// <param name="weatherFactor">weather factor 1 - best weather</param>
        /// <param name="sunangle">Sun angle</param>
        /// <param name="panelpowerangle">Sun To Panel angle of Panel Power peak measurement</param>
        /// <returns></returns>
        /// <exception cref="DivideByZeroException"></exception>
        public static double GetPeakPowerOfPanel(double panelPeakPower, double efficiency, double weatherFactor, double sunangle, double panelpowerangle)
        {
            if (panelpowerangle > 0)
                return panelPeakPower * efficiency * weatherFactor * (sunangle / panelpowerangle);
            else
                throw new DivideByZeroException();
        }
    }
}

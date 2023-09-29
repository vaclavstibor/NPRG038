using cAlgo.API;
using System.Collections.Generic;

namespace cAlgo
{
    ///<summary>
    /// This indicator calculates the Exponential Moving Average (EMA) of a specified data series.
    /// It provides a smoothed representation of price data over a specified period.
    ///</summary>
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC, AutoRescale = false, AccessRights = AccessRights.None)]
    public class MediumEMA : Indicator
    {
        [Parameter("Source")]
        public DataSeries Source { get; set; }

        [Parameter("Periods", DefaultValue = 40)]
        public int Periods { get; set; }

        [Output("Main", LineColor = "Green")]
        public IndicatorDataSeries Result { get; set; }

        private double exp;

        // Array to store the last 10 EMA values for reference
        public double[] last10EMAValues = new double[10];

        protected override void Initialize()
        {
            // Calculate the exponential smoothing factor
            exp = 2.0 / (Periods + 1);

            // Initialize the array to store the last 10 EMA values
            last10EMAValues = new double[10];
        }

        ///<summary>
        /// This method calculates the EMA at the specified index based on the source data series.
        ///</summary>
        public override void Calculate(int index)
        {
            // Get the previous EMA value
            var previousValue = Result[index - 1];

            // Check if the previous value is NaN (not-a-number)
            if (double.IsNaN(previousValue))
                Result[index] = Source[index]; // Use the source value as the initial EMA value
            else
                Result[index] = Source[index] * exp + previousValue * (1 - exp); // Calculate the EMA using the smoothing formula

            // Print the last 10 EMA values for debugging or analysis
            for (int i = 0; i < 10; i++)
            {
                last10EMAValues[i] = Result[index - i];
                //Print("Last 10 EMA Values[" + i + "]: " + last10EMAValues[i]);
            }
            
            //Print("Last Medium EMA Value" + last10EMAValues[1]);
        }
    }
}

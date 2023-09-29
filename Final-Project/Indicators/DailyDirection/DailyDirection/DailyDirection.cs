using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo
{
    /// <summary>
    /// A struct representing a candlestick with various attributes and properties.
    /// </summary>
    public struct Candle
    {
        // Enumeration to represent candlestick direction (Negative or Positive).
        public enum EDirection { Negative, Positive };

        // Properties to store various attributes of a candlestick.
        public double Low { get; }           // Lowest price during the candle's time period.
        public double High { get; }          // Highest price during the candle's time period.
        public double Open { get; }          // Opening price of the candle.
        public double Close { get; }         // Closing price of the candle.
        public double MidPoint { get; }      // Midpoint price between High and Low.
        public DateTime Time { get; }        // Timestamp representing the candle's time.
        public bool IsDoji { get; }          // Boolean indicating whether the candle is a Doji.
        public EDirection Direction { get; }

        // Constructor for creating a Candle object from Bars data and a specified shift.
        public Candle(Bars bars, int shift)
        {
            // Check if the shift is within a valid range of data in "bars".
            if (shift >= 0 && shift < bars.ClosePrices.Count)
            {
                // Initialize properties based on the data at the specified shift.
                Low = bars.Last(shift).Low;
                High = bars.Last(shift).High;
                Open = bars.Last(shift).Open;
                Close = bars.Last(shift).Close;
                MidPoint = (High + Low) / 2.0;  // Calculate MidPoint before initializing Direction.
                Time = bars.Last(1).OpenTime;
                IsDoji = Math.Abs(Open - Close) <= 0.0001;

                // Initialize Direction based on Close and MidPoint.
                Direction = (Close > MidPoint) ? EDirection.Positive : EDirection.Negative;
            }
            else
            {
                // Handle the case where the shift is out of range by setting properties to default values.
                Low = High = Open = Close = MidPoint = 0.0;
                Time = DateTime.MinValue;
                IsDoji = false;
                Direction = EDirection.Negative; // Set a default direction in this case.
            }
        }
    }

    /// <summary>
    /// An indicator that displays the direction of daily candles using up and down arrows on the chart.
    /// </summary>
    [Indicator(AccessRights = AccessRights.None)]
    public class DailyDirection : Indicator
    {    
        private Candle DailyCandle;

        private int DailyBarsCount;

        protected override void Initialize() { }

        /// <summary>
        /// This method is called by the cAlgo platform to calculate the indicator values and visualize daily candle direction.
        /// </summary>
        /// <param name="index">The current index of the price data being processed.</param>
        public override void Calculate(int index)
        {
            var DailyBars  = MarketData.GetBars(TimeFrame.Daily, Symbol.Name);
            var CurrentBars = MarketData.GetBars(TimeFrame, Symbol.Name);
            
            if (DailyBarsCount != DailyBars.Count)
            {
                DailyCandle = new(DailyBars,1);
                
                switch (DailyCandle.Direction)
                {
                    case Candle.EDirection.Negative:
                        // Draw a down arrow icon on the chart for a negative daily candle
                        Chart.DrawIcon(DailyCandle.Time.ToString(), ChartIconType.DownArrow, DailyCandle.Time, DailyCandle.High, Color.Red);
                        break;
                    case Candle.EDirection.Positive:
                        // Draw an up arrow icon on the chart for a positive daily candle
                        Chart.DrawIcon(DailyCandle.Time.ToString(), ChartIconType.UpArrow, DailyCandle.Time, DailyCandle.Low, Color.Blue);
                        break;
                    default:
                        break;
                }

                // Update the count of daily bars
                DailyBarsCount = MarketData.GetBars(TimeFrame.Weekly, Symbol.Name).Count;
            }       
            
            // Calculate additional values if needed for the indicator
            // Result[index] = ...
        }
    }
}

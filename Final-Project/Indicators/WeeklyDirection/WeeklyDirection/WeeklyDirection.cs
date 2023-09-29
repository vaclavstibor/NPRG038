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
                MidPoint = (High + Low) / 2.0;
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
    /// An indicator that displays the weekly candlestick's direction and a rectangle on the chart to represent the weekly trading range.
    /// </summary>
    [Indicator(AccessRights = AccessRights.None)]
    public class WeeklyDirection : Indicator
    {
        private Candle WeeklyCandle;
        private Candle CurrentCandle;

        private int WeeklyBarsCount;
        private int CurrentBarsCount;

        public double Max;
        public double Min;

        protected override void Initialize() { }
        
        /// <summary>
        /// This method is an override required by the cAlgo indicator framework and is automatically called during price data calculation.
        /// It calculates and updates the indicator's values, visualizes the weekly trading range using rectangles on the chart, and tracks important price levels.
        /// </summary>
        /// <param name="index">The current index of the price data being processed.</param>
        public override void Calculate(int index)
        {
            // Retrieve historical price data for the weekly and current timeframes
            var WeeklyBars = MarketData.GetBars(TimeFrame.Weekly, Symbol.Name);
            var CurrentBars = MarketData.GetBars(TimeFrame, Symbol.Name);
        
            // Check if the number of weekly bars has changed, indicating a new weekly candle
            if (WeeklyBarsCount != WeeklyBars.Count)
            {
                // Create a Candle object representing the latest weekly candle
                WeeklyCandle = new(WeeklyBars, 1);
                
                // Initialize Max and Min for the weekly trading range
                Max = WeeklyCandle.High;
                Min = WeeklyCandle.Low;
                
                // Determine the color based on the direction of the weekly candle
                Color clr;
        
                switch (WeeklyCandle.Direction)
                {
                    case Candle.EDirection.Negative:
                        clr = Color.Red;
                        break;
                    case Candle.EDirection.Positive:
                        clr = Color.Blue;
                        break;
                    default:
                        clr = Color.White;
                        break;
                }
        
                // Calculate the start and end times for the rectangle representing the weekly trading range
                DateTime time1 = WeeklyCandle.Time.AddDays(7);
                DateTime time2 = WeeklyCandle.Time.AddDays(14);
        
                // Draw a rectangle on the chart to represent the weekly trading range
                Chart.DrawRectangle(WeeklyCandle.Time.ToString(),
                                    time1,
                                    Max,
                                    time2,
                                    Min,
                                    clr);
        
                // Print the Min and Max values for reference
                Print(Min, Max);
        
                // Update the count of weekly bars
                WeeklyBarsCount = MarketData.GetBars(TimeFrame.Weekly, Symbol.Name).Count;
            }
        
            // Check if the number of current bars has changed
            if (CurrentBarsCount != CurrentBars.Count)
            {
                // Create a Candle object representing the latest current candle
                CurrentCandle = new(CurrentBars, 1);
        
                // Update Max if the current high exceeds the current Max
                if (CurrentCandle.High > Max)
                {
                    Max = CurrentCandle.High;
        
                    // Find and update the existing chart rectangle's Y1 coordinate to adjust for the new Max value
                    var obj = from chartObject in Chart.Objects
                              where chartObject.Name.StartsWith(WeeklyCandle.Time.ToString())
                              select chartObject;
        
                    if (obj is ChartRectangle rectangle)
                    {
                        rectangle.Y1 = Max;
                    }
                }
        
                // Update Min if the current low is below the current Min
                if (CurrentCandle.Low < Min)
                {
                    Min = CurrentCandle.Low;
        
                    // Find and update the existing chart rectangle's Y2 coordinate to adjust for the new Min value
                    var obj = from chartObject in Chart.Objects
                              where chartObject.Name.StartsWith(WeeklyCandle.Time.ToString())
                              select chartObject;
        
                    // Print the selected objects for debugging purposes
                    Print(obj);
        
                    if (obj is ChartRectangle rectangle)
                    {
                        rectangle.Y2 = Min;
                    }
                }
        
                // Update the count of current bars
                CurrentBarsCount = MarketData.GetBars(TimeFrame, Symbol.Name).Count;
        
                // Print the current low and Min values for reference
                Print(CurrentCandle.Low, "LOW");
                Print(Min, "MIN");
            }
            // Calculate additional values if needed for the indicator
            // Result[index] = ...
        }

    }
}

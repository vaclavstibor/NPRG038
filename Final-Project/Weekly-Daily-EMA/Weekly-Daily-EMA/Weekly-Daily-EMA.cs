using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;

namespace cAlgo.Robots
{
    public struct RSI
    {
        DataSeries Source { get; set; }
        int Period { get; set; }
    
        public RSI(DataSeries source, int period)
        {
            Source = source;
            Period = period;
        }
    }
    
    [Robot(AccessRights = AccessRights.None)]
    public class WeeklyDailyEMA : Robot
    {
        #region Money Management
        [Parameter("Initial Quantity (Lots)", Group = "Money Management", DefaultValue = 1, MinValue = 0.01, Step = 0.01)]
        public double ExtVolume { get; set; }

        [Parameter("Stop Loss", Group = "Money Management", DefaultValue = 40)]
        public int ExtStopLoss { get; set; }

        [Parameter("Take Profit", Group = "Money Management", DefaultValue = 40)]
        public int ExtTakeProfit { get; set; }
        #endregion

        #region Partial Close
        [Parameter("Allow Positions partial close", Group = "Partial Close", DefaultValue = false)]
        public bool ExtAllowPartialClose { get; set; }
        
        [Parameter("Distance for first partial close", Group = "Partial Close", DefaultValue = 15)]
        public int ExtPartialCloseFirstDistance { get; set; }

        [Parameter("Size of first partial close", Group = "Partial Close", DefaultValue = 0.4)]
        public double ExtPartialCloseFirstAmount { get; set; }
        
        [Parameter("Distance for second partial close", Group = "Partial Close", DefaultValue = 30)]
        public int ExtPartialCloseSecondDistance { get; set; }

        [Parameter("Size of second partial close", Group = "Partial Close", DefaultValue = 0.3)]
        public double ExtPartialCloseSecondAmout { get; set; }
        #endregion

        #region Trailing Stop
        [Parameter("Allow Positions trailing stop", Group = "Trailing Stop", DefaultValue = false)]
        public bool ExtAllowTrailingStop { get; set; }
        
        [Parameter("Trail point", Group = "Trailing Stop", DefaultValue = 30)]
        public int ExtTrailPoint { get; set; }
        #endregion Trailing Stop

        #region Trading Hours
        [Parameter("Trading hour start", Group = "Trading Hours", DefaultValue = 7)]
        public int TradingHourStart { get; set; }
        
        [Parameter("Trading hour stop", Group = "Trading Hours", DefaultValue = 19)]
        public int TradingHourStop { get; set; }
        #endregion
        
        #region RSI
        private RelativeStrengthIndex RSI;

        [Parameter("Source", Group = "RSI")]
        public DataSeries RSI_Source { get; set; }

        [Parameter("Periods", Group = "RSI", DefaultValue = 14)]
        public int RSI_Period { get; set; }
        #endregion

        protected override void OnStart()
        {
            RSI = Indicators.RelativeStrengthIndex(RSI_Source, RSI_Period);
        }

        protected override void OnTick()
        {                                                

        }

        protected override void OnStop()
        {
            // Handle cBot stop here
        }
        
        private void Close(TradeType tradeType)
        {
            foreach (var position in Positions.FindAll("SampleRSI", SymbolName, tradeType))
                ClosePosition(position);
        }

        private void Open(TradeType tradeType)
        {
            var position = Positions.Find("SampleRSI", SymbolName, tradeType);
            var volumeInUnits = Symbol.QuantityToVolumeInUnits(0.1);

            if (position == null)
                ExecuteMarketOrder(tradeType, SymbolName, volumeInUnits, "SampleRSI");
        }
    }
}                             
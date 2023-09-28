using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using cAlgo.API;
using cAlgo.API.Collections;
using cAlgo.API.Indicators;
using cAlgo.API.Internals;


namespace cAlgo.Robots
{
    public struct RSI
    {
        DataSeries Source { get; set; }
        int TimeFrame { get; set; }
        
        public RSI(DataSeries source, int timeFrame)
        {
            Source = source;
            TimeFrame = timeFrame;
        }
    }
    
    public struct MA
    {
        DataSeries Source { get; set; }
        int TimeFrame { get; set; }
        int Type { get; set; } 
        int Method { get; set; }        // type
        int AveragePeriod { get; set; }
        
        public MA(DataSeries source, int timeFrame, int type, int method, int averagePeriod)
        {
            Source = source;
            TimeFrame = timeFrame;
            Type = type;
            Method = method;
            AveragePeriod = averagePeriod;
        }
    }
    
    public static class MovingAverages
    {
        public static double[] Fast { get; set; }
        public static double[] Medium { get; set; }
        public static double[] Slow { get; set; }
    }

    /// <summary>
    /// Represents a collection of different candlestick data used in financial analysis.
    /// This struct contains candlestick data for various timeframes, including fast, medium, slow, daily (D1),
    /// weekly (W1), and the current timeframe.
    /// </summary>
    public struct Candles
    {
        /// <summary>
        /// Gets or sets the candlestick data for the fast timeframe.
        /// </summary>
        public Candle Fast { get; set; }

        /// <summary>
        /// Gets or sets the candlestick data for the medium timeframe.
        /// </summary>
        public Candle Medium { get; set; }

        /// <summary>
        /// Gets or sets the candlestick data for the slow timeframe.
        /// </summary>
        public Candle Slow { get; set; }

        /// <summary>
        /// Gets or sets the candlestick data for the daily (D1) timeframe.
        /// </summary>
        public Candle D1 { get; set; }

        /// <summary>
        /// Gets or sets the candlestick data for the weekly (W1) timeframe.
        /// </summary>
        public Candle W1 { get; set; }

        /// <summary>
        /// Gets or sets the candlestick data for the current timeframe.
        /// </summary>
        public Candle Current { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Candles"/> struct with candlestick data for different timeframes.
        /// </summary>
        /// <param name="fast">The candlestick data for the fast timeframe.</param>
        /// <param name="medium">The candlestick data for the medium timeframe.</param>
        /// <param name="slow">The candlestick data for the slow timeframe.</param>
        /// <param name="d1">The candlestick data for the daily (D1) timeframe.</param>
        /// <param name="w1">The candlestick data for the weekly (W1) timeframe.</param>
        /// <param name="current">The candlestick data for the current timeframe.</param>
        public Candles(Candle fast, Candle medium, Candle slow, Candle d1, Candle w1, Candle current)
        {
            Fast = fast;
            Medium = medium;
            Slow = slow;
            D1 = d1;
            W1 = w1;
            Current = current;
        }
    }


    /// <summary>
    /// Represents a candlestick, which is a structural unit used in financial analysis.
    /// A candlestick contains various attributes, including price information and indicators.
    /// </summary>
    public struct Candle
    {
        // Enumeration to represent candlestick direction (Negative or Positive).
        public enum EDirection { Negative, Positive };

        // Properties to store various attributes of a candlestick.

        /// <summary>
        /// Gets the lowest price during the candle's time period.
        /// </summary>
        public double Low { get; }

        /// <summary>
        /// Gets the highest price during the candle's time period.
        /// </summary>
        public double High { get; }

        /// <summary>
        /// Gets the opening price of the candle.
        /// </summary>
        public double Open { get; }

        /// <summary>
        /// Gets the closing price of the candle.
        /// </summary>
        public double Close { get; }

        /// <summary>
        /// Gets the midpoint price between High and Low.
        /// </summary>
        public double MidPoint { get; }

        /// <summary>
        /// Gets the timestamp representing the candle's open time.
        /// </summary>
        public DateTime Time { get; }

        /// <summary>
        /// Gets a boolean indicating whether the candle is a Doji.
        /// </summary>
        public bool IsDoji { get; }

        /// <summary>
        /// Gets the direction of the candle (Positive or Negative).
        /// </summary>
        public EDirection Direction { get; }

        /// <summary>
        /// Gets or sets a boolean indicating whether the candle has been traded.
        /// </summary>
        public bool Traded { get; set; }

        //--- Close conditions for Fast Moving Average
        public bool CloseLongFastMA { get; }
        public bool CloseShortFastMA { get; }

        //--- Close conditions for Medium Moving Average
        public bool CloseLongMediumMA { get; }
        public bool CloseShortMediumMA { get; }

        //--- Close conditions for Slow Moving Average
        public bool CloseLongSlowMA { get; }
        public bool CloseShortSlowMA { get; }

        /// <summary>
        /// Constructor for creating a Candle object from Bars data and a specified shift.
        /// </summary>
        /// <param name="bars">The Bars data used to construct the candle.</param>
        /// <param name="shift">The shift indicating which data point to use from the Bars data.</param>
        public Candle(Bars bars, int shift)
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

            CloseLongFastMA = CheckCloseFastMA(TradeType.Buy, Close);
            CloseShortFastMA = CheckCloseFastMA(TradeType.Sell, Close);

            CloseLongMediumMA = CheckCloseMediumMA(TradeType.Buy, Close);
            CloseShortMediumMA = CheckCloseMediumMA(TradeType.Sell, Close);

            CloseLongSlowMA = CheckCloseSlowMA(TradeType.Buy, Close);
            CloseShortSlowMA = CheckCloseSlowMA(TradeType.Sell, Close);

            Traded = false;
        }

        /// <summary>
        /// Checks whether the candle should be closed based on Fast Moving Average and trade type.
        /// </summary>
        /// <param name="tradeType">The type of trade (Buy or Sell).</param>
        /// <param name="close">The closing price of the candle.</param>
        /// <returns>True if the candle should be closed; otherwise, false.</returns>
        private static bool CheckCloseFastMA(TradeType tradeType, double close)
        {
            switch (tradeType)
            {
                case TradeType.Sell:
                    if (close < MovingAverages.Fast[1])
                    {
                        return true;
                    }
                    break;
                case TradeType.Buy:
                    if (close > MovingAverages.Fast[1])
                    {
                        return true;
                    }
                    break;
            }

            return false;
        }

        /// <summary>
        /// Checks whether the candle should be closed based on Medium Moving Average and trade type.
        /// </summary>
        /// <param name="tradeType">The type of trade (Buy or Sell).</param>
        /// <param name="close">The closing price of the candle.</param>
        /// <returns>True if the candle should be closed; otherwise, false.</returns>
        private static bool CheckCloseMediumMA(TradeType tradeType, double close)
        {
            switch (tradeType)
            {
                case TradeType.Sell:
                    if (close < MovingAverages.Medium[1])
                    {
                        return true;
                    }
                    break;
                case TradeType.Buy:
                    if (close > MovingAverages.Medium[1])
                    {
                        return true;
                    }
                    break;
            }

            return false;
        }

        /// <summary>
        /// Checks whether the candle should be closed based on Slow Moving Average and trade type.
        /// </summary>
        /// <param name="tradeType">The type of trade (Buy or Sell).</param>
        /// <param name="close">The closing price of the candle.</param>
        /// <returns>True if the candle should be closed; otherwise, false.</returns>
        private static bool CheckCloseSlowMA(TradeType tradeType, double close)
        {
            switch (tradeType)
            {
                case TradeType.Sell:
                    if (close < MovingAverages.Slow[1])
                    {
                        return true;
                    }
                    break;
                case TradeType.Buy:
                    if (close > MovingAverages.Slow[1])
                    {
                        return true;
                    }
                    break;
            }

            return false;
        }
    }

    /// <summary>
    /// Represents a trading robot designed for analyzing and executing trading strategies based on EMA (Exponential Moving Average) indicators
    /// and other parameters. This robot operates on different timeframes and implements various setups and money management rules.
    /// </summary>
    [Robot(AccessRights = AccessRights.None)]
    public class WeeklyDailyEMA : Robot
    {

        #region Setups
        /// <summary>
        /// Determines whether to allow the A setup for trading. Enabling this setup may trigger specific trading conditions.
        /// </summary>
        [Parameter("Allow A Setup", Group = "Setups", DefaultValue = true)]
        public bool ExtAllowASetup { get; set; }

        /// <summary>
        /// Determines whether to allow the B setup for trading. Enabling this setup may trigger specific trading conditions.
        /// </summary>
        [Parameter("Allow B Setup", Group = "Setups", DefaultValue = true)]
        public bool ExtAllowBSetup { get; set; }

        /// <summary>
        /// Determines whether to allow the C setup for trading. Enabling this setup may trigger specific trading conditions.
        /// </summary>
        [Parameter("Allow C Setup", Group = "Setups", DefaultValue = true)]
        public bool ExtAllowCSetup { get; set; }
        #endregion



        #region CandlesTimeframes
        /// <summary>
        /// The fast timeframe used for analyzing market data. This parameter is used to define the speed of analysis.
        /// </summary>
        [Parameter("Fast Timeframe", Group = "Candles Timeframes", DefaultValue = "Minute15")]
        public TimeFrame FastTimeframe { get; set; }

        /// <summary>
        /// The medium timeframe used for analyzing market data. This parameter is used for intermediate-term analysis.
        /// </summary>
        [Parameter("Medium Timeframe", Group = "Candles Timeframes", DefaultValue = "Hour1")]
        public TimeFrame MediumTimeframe { get; set; }

        /// <summary>
        /// The slow timeframe used for analyzing market data. This parameter is used for longer-term analysis.
        /// </summary>
        [Parameter("Slow Timeframe", Group = "Candles Timeframes", DefaultValue = "Hour4")]
        public TimeFrame SlowTimeframe { get; set; }
        #endregion



        #region Money Management
        /// <summary>
        /// The initial trading volume in lots. This parameter is used for money management.
        /// </summary>
        [Parameter("Initial Quantity (Lots)", Group = "Money Management", DefaultValue = 1, MinValue = 0.01, Step = 0.01)]
        public double ExtVolume { get; set; }

        /// <summary>
        /// The stop loss value in pips. This parameter is used for setting stop loss levels.
        /// </summary>
        [Parameter("Stop Loss", Group = "Money Management", DefaultValue = 40)]
        public int ExtStopLoss { get; set; }

        /// <summary>
        /// The take profit value in pips. This parameter is used for setting take profit levels.
        /// </summary>
        [Parameter("Take Profit", Group = "Money Management", DefaultValue = 40)]
        public int ExtTakeProfit { get; set; }
        #endregion



        /// <summary>
        /// This region contains parameters related to identifying pullbacks in moving averages.
        /// </summary>
        #region PullBacks
        
        /// <summary>
        /// The threshold for detecting pullbacks in the Fast Moving Average.
        /// </summary>
        [Parameter("Fast MA PullBack ThresHold", Group = "PullBacks", DefaultValue = 1)]
        public double ExtFastThresHold { get; set; }
        
        /// <summary>
        /// The threshold for detecting pullbacks in the Medium Moving Average.
        /// </summary>
        [Parameter("Medium MA PullBack ThresHold", Group = "PullBacks", DefaultValue = 1)]
        public double ExtMediumThresHold { get; set; }
        
        /// <summary>
        /// The threshold for detecting pullbacks in the Slow Moving Average.
        /// </summary>
        [Parameter("Slow MA PullBack ThresHold", Group = "PullBacks", DefaultValue = 1)]
        public double ExtSlowThresHold { get; set; }
        
        #endregion

        
        
        /// <summary>
        /// This region contains parameters related to partial closing of positions.
        /// </summary>
        #region Partial Close
        
        /// <summary>
        /// Allows partial closing of positions when set to true.
        /// </summary>
        [Parameter("Allow Positions partial close", Group = "Partial Close", DefaultValue = false)]
        public bool ExtAllowPartialClose { get; set; }
        
        /// <summary>
        /// The distance at which the first partial close will be executed.
        /// </summary>
        [Parameter("Distance for first partial close", Group = "Partial Close", DefaultValue = 15)]
        public int ExtPartialCloseFirstDistance { get; set; }
        
        /// <summary>
        /// The size or amount of the first partial close as a percentage of the position size.
        /// </summary>
        [Parameter("Size of first partial close", Group = "Partial Close", DefaultValue = 0.4)]
        public double ExtPartialCloseFirstAmount { get; set; }
        
        /// <summary>
        /// The distance at which the second partial close will be executed.
        /// </summary>
        [Parameter("Distance for second partial close", Group = "Partial Close", DefaultValue = 30)]
        public int ExtPartialCloseSecondDistance { get; set; }
        
        /// <summary>
        /// The size or amount of the second partial close as a percentage of the position size.
        /// </summary>
        [Parameter("Size of second partial close", Group = "Partial Close", DefaultValue = 0.3)]
        public double ExtPartialCloseSecondAmount { get; set; }
        
        #endregion


        /// <summary>
        /// This region contains parameters related to trailing stops for positions.
        /// </summary>
        #region Trailing Stop
        
        /// <summary>
        /// Allows trailing stop for positions when set to true.
        /// </summary>
        [Parameter("Allow Positions trailing stop", Group = "Trailing Stop", DefaultValue = false)]
        public bool ExtAllowTrailingStop { get; set; }
        
        /// <summary>
        /// The trail point, which is the distance at which the trailing stop is activated.
        /// </summary>
        [Parameter("Trail point", Group = "Trailing Stop", DefaultValue = 30)]
        public int ExtTrailPoint { get; set; }
        
        #endregion
        
        /// <summary>
        /// This region contains parameters related to trading hours.
        /// </summary>
        #region Trading Hours
        
        /// <summary>
        /// The start hour for trading activities.
        /// </summary>
        [Parameter("Trading hour start", Group = "Trading Hours", DefaultValue = 7)]
        public int TradingHourStart { get; set; }
        
        /// <summary>
        /// The stop hour for trading activities.
        /// </summary>
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


        /// <summary>
        /// This region contains parameters and indicators related to moving averages (MA).
        /// </summary>
        #region MA
        
        // Instances of different Exponential Moving Average (EMA) indicators
        private FastEMA FastEMA;
        private MediumEMA MediumEMA;
        private SlowEMA SlowEMA;
        
        /// <summary>
        /// The data series used as the source for the moving averages.
        /// </summary>
        //[Parameter("Source", Group = "MA")]
        //public DataSeries MA_Source { get; set; }
        
        /// <summary>
        /// The period for the Fast Exponential Moving Average (EMA).
        /// </summary>
        [Parameter("Fast EMA Periods", Group = "MA", DefaultValue = 10)]
        public int FastMAPeriod { get; set; }
        
        /// <summary>
        /// The period for the Medium Exponential Moving Average (EMA).
        /// </summary>
        [Parameter("Medium EMA Periods", Group = "MA", DefaultValue = 40)]
        public int MediumMAPeriod { get; set; }
        
        /// <summary>
        /// The period for the Slow Exponential Moving Average (EMA).
        /// </summary>
        [Parameter("Slow EMA Periods", Group = "MA", DefaultValue = 160)]
        public int SlowMAPeriod { get; set; }
        
        #endregion
        
        
        /// <summary>
        /// This method is called when the trading robot is started, and it initializes various components and parameters required for trading.
        /// </summary>
        protected override void OnStart()
        {
            #if DEBUG
            // Launch a debugger if the DEBUG preprocessor directive is defined.
            System.Diagnostics.Debugger.Launch();
            #endif

            RSI = Indicators.RelativeStrengthIndex(RSI_Source, RSI_Period);
            
            FastEMA = Indicators.GetIndicator<FastEMA>(Bars.ClosePrices, FastMAPeriod);
            MediumEMA = Indicators.GetIndicator<MediumEMA>(Bars.ClosePrices, MediumMAPeriod);
            SlowEMA = Indicators.GetIndicator<SlowEMA>(Bars.ClosePrices, SlowMAPeriod);
            
            Print(String.Format("Last Fast/Medium/Slow Moving Average value: {0} / {1} / {2}", FastEMA.Result.LastValue, MediumEMA.Result.LastValue, SlowEMA.Result.LastValue));
            
            MovingAverages.Fast = FastEMA.last10EMAValues;
            MovingAverages.Medium = MediumEMA.last10EMAValues;
            MovingAverages.Slow = SlowEMA.last10EMAValues;
            
            a = new A(this);
            b1 = new B1(this);
            b2 = new B2(this);
            c = new C(this);
        }
        
        private A  a;
        private B1 b1;
        private B2 b2;
        private C  c;
        
        int CurrentBarsCount;
        int FastBarsCount;
        int MediumBarsCount;
        int SlowBarsCount;
        int W1BarsCount;
        int D1BarsCount;
        
        private Candle D1Candle;
        private Candle W1Candle;
        
        private Candle FastCandle;
        private Candle MediumCandle;
        private Candle SlowCandle;
        
        private Candle CurrentCandle;

        /// <summary>
        /// This method is called on every tick, and it handles various tasks related to monitoring and updating candlestick data,
        /// setting up trading strategies, and executing partial position closures.
        /// </summary>
        protected override void OnTick()
        {
            // Check if the number of bars in the Fast timeframe has changed.
            if (FastBarsCount != MarketData.GetBars(FastTimeframe, Symbol.Name).Count)
            {
                // Create a new FastCandle using the latest data from the Fast timeframe.
                FastCandle = new Candle(MarketData.GetBars(FastTimeframe, Symbol.Name), 1);

                // Update the FastBarsCount to reflect the current number of bars.
                FastBarsCount = MarketData.GetBars(FastTimeframe, Symbol.Name).Count;

                // Print a message to indicate the arrival of a new Fast candle.
                Print("New Fast candle.");
            }

            // Repeat the above process for the Medium timeframe.
            if (MediumBarsCount != MarketData.GetBars(MediumTimeframe, Symbol.Name).Count)
            {
                MediumCandle = new Candle(MarketData.GetBars(MediumTimeframe, Symbol.Name), 1);
                MediumBarsCount = MarketData.GetBars(MediumTimeframe, Symbol.Name).Count;
                Print("New Medium candle.");
            }

            // Repeat the above process for the Slow timeframe.
            if (SlowBarsCount != MarketData.GetBars(SlowTimeframe, Symbol.Name).Count)
            {
                SlowCandle = new Candle(MarketData.GetBars(SlowTimeframe, Symbol.Name), 1);
                SlowBarsCount = MarketData.GetBars(SlowTimeframe, Symbol.Name).Count;
                Print("New Slow candle.");
            }

            // Repeat the above process for the Daily timeframe.
            if (D1BarsCount != MarketData.GetBars(TimeFrame.Daily, Symbol.Name).Count)
            {
                D1Candle = new Candle(MarketData.GetBars(TimeFrame.Daily, Symbol.Name), 1);
                D1BarsCount = MarketData.GetBars(TimeFrame.Daily, Symbol.Name).Count;

                // Print a message to indicate the arrival of a new Daily (D1) candle and its direction.
                Print("New D1 candle.");
                Print("D1Candle direction is ", D1Candle.Direction);
            }

            // Repeat the above process for the Weekly timeframe.
            if (W1BarsCount != MarketData.GetBars(TimeFrame.Weekly, Symbol.Name).Count)
            {
                W1Candle = new Candle(MarketData.GetBars(TimeFrame.Weekly, Symbol.Name), 1);
                W1BarsCount = MarketData.GetBars(TimeFrame.Weekly, Symbol.Name).Count;

                // Print a message to indicate the arrival of a new Weekly (W1) candle and its direction.
                Print("New W1 candle.");
                Print("W1Candle direction is ", W1Candle.Direction);
            }

            // Check if the number of bars in the current timeframe has changed.
            if (CurrentBarsCount != MarketData.GetBars(TimeFrame, Symbol.Name).Count)
            {
                // Create a new CurrentCandle using the latest data from the current timeframe.
                CurrentCandle = new Candle(MarketData.GetBars(TimeFrame, Symbol.Name), 1);

                // Update the CurrentBarsCount to reflect the current number of bars.
                CurrentBarsCount = MarketData.GetBars(TimeFrame, Symbol.Name).Count;

                // Print a message to indicate the arrival of a new Current candle.
                Print("New Current candle.");
            }

            // Create a Candles object to store the various candlesticks for use in trading strategy setups.
            Candles candles = new(FastCandle, MediumCandle, SlowCandle, D1Candle, W1Candle, CurrentCandle);

            // Call the setup methods of trading strategies A, B1, B2, and C with the candle data and setup configuration flags.
            a.Setup(candles, ExtAllowASetup);
            b1.Setup(candles, ExtAllowBSetup);
            b2.Setup(candles, ExtAllowBSetup);
            c.Setup(candles, ExtAllowBSetup);


            // Execute partial position closures if the partial close feature is enabled.
            ExecutePartialClose();
            ExecuteTrailingStop();
        }


        /// <summary>
        /// This method is called when the trading robot is stopped or deactivated, and it can be used to perform cleanup or finalize any operations.
        /// </summary>
        protected override void OnStop()
        {
            // Handle cBot stop here
        }

        /// <summary>
        /// Determines whether it is allowed to open a Buy position based on the maximum allowed count of open Buy positions.
        /// </summary>
        /// <param name="allowedCount">The maximum allowed count of open Buy positions.</param>
        /// <returns>True if opening a Buy position is allowed; otherwise, false.</returns>
        private bool IsAllowedToOpenBuyPos(int allowedCount)
        {
            // Count the number of currently open Buy positions for the current trading symbol.
            int numberOfLongPositions = Positions.Where(pos => pos.Symbol == Symbol)
                                                 .Where(pos => pos.TradeType == TradeType.Buy)
                                                 .Count();

            // Check if the number of open Buy positions is less than the allowed count.
            if (numberOfLongPositions >= allowedCount)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether it is allowed to open a Sell position based on the maximum allowed count of open Sell positions.
        /// </summary>
        /// <param name="allowedCount">The maximum allowed count of open Sell positions.</param>
        /// <returns>True if opening a Sell position is allowed; otherwise, false.</returns>
        private bool IsAllowedToOpenSellPos(int allowedCount)
        {
            // Count the number of currently open Sell positions for the current trading symbol.
            int numberOfLongPositions = Positions.Where(pos => pos.Symbol == Symbol)
                                                 .Where(pos => pos.TradeType == TradeType.Sell)
                                                 .Count();

            // Check if the number of open Sell positions is less than the allowed count.
            if (numberOfLongPositions >= allowedCount)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Executes partial closing of open positions if the partial close feature is enabled.
        /// </summary>
        private void ExecutePartialClose()
        {
            // Check if partial close is allowed.
            if (!ExtAllowPartialClose)
                return;

            // Iterate through all open positions.
            foreach (var position in Positions)
            {
                // Skip positions that belong to a different trading symbol.
                if (position.SymbolName != Symbol.Name)
                    continue;

                if (position.TradeType == TradeType.Buy)
                {
                    // Check if the price difference exceeds the first partial close distance and the position volume matches the initial volume.
                    if ((Symbol.Bid - position.EntryPrice) > ExtPartialCloseFirstDistance * Symbol.PipSize && position.VolumeInUnits == ExtVolume)
                    {
                        // Perform the first partial close by selling a portion of the position.
                        ClosePosition(position, position.VolumeInUnits * ExtPartialCloseFirstAmount);
                    }
                }
                else if (position.TradeType == TradeType.Sell)
                {
                    // Check if the price difference exceeds the first partial close distance and the position volume matches the initial volume.
                    if ((position.EntryPrice - Symbol.Ask) > ExtPartialCloseFirstDistance * Symbol.PipSize && position.VolumeInUnits == ExtVolume)
                    {
                        // Perform the first partial close by buying back a portion of the position.
                        ClosePosition(position, position.VolumeInUnits * ExtPartialCloseFirstAmount);
                    }
                }
            }

            // Iterate through all open positions again for potential second partial close.
            foreach (var position in Positions)
            {
                // Skip positions that belong to a different trading symbol.
                if (position.SymbolName != Symbol.Name)
                    continue;

                if (position.TradeType == TradeType.Buy)
                {
                    // Check if the price difference exceeds the second partial close distance.
                    if ((Symbol.Bid - position.EntryPrice) > ExtPartialCloseSecondDistance * Symbol.PipSize)
                    {
                        // Perform the second partial close by selling a portion of the position.
                        ClosePosition(position, position.VolumeInUnits * ExtPartialCloseSecondAmount);
                    }
                }
                else if (position.TradeType == TradeType.Sell)
                {
                    // Check if the price difference exceeds the second partial close distance.
                    if ((position.EntryPrice - Symbol.Ask) > ExtPartialCloseSecondDistance * Symbol.PipSize)
                    {
                        // Perform the second partial close by buying back a portion of the position.
                        ClosePosition(position, position.VolumeInUnits * ExtPartialCloseSecondAmount);
                    }
                }
            }
        }

        /// <summary>
        /// This method is responsible for executing a trailing stop for open positions, if the trailing stop feature is enabled.
        /// It iterates through all open positions and adjusts their stop loss and take profit levels based on predefined conditions.
        /// We suppose that opened positions are 1:2 RRR. 
        /// </summary>
        private void ExecuteTrailingStop()
        {
            // Check if the trailing stop feature is enabled
            if (ExtAllowTrailingStop)
            {
                // Iterate through all open positions in reverse order
                for (int i = Positions.Count - 1; i >= 0; i--)
                {
                    // Get the current position
                    var position = Positions[i];

                    // Determine the type of the position (Buy or Sell)
                    var type = position.TradeType;

                    // Get the current stop loss and take profit values or set them to 0 if not defined
                    double currentStopLoss = position.StopLoss ?? 0;
                    double stopLoss;
                    double currentTakeProfit = position.TakeProfit ?? 0;
                    double takeProfit;

                    // Get the opening price of the position
                    double openPrice = position.EntryPrice;

                    // Check if the position is a Buy position
                    if (type == TradeType.Buy)
                    {
                        // Determine whether to trail the stop loss based on the current stop loss relative to the opening price
                        if (currentStopLoss >= openPrice)
                        {
                            // Calculate the new stop loss and take profit levels with trailing applied
                            stopLoss = currentStopLoss + 2 * ExtTrailPoint * Symbol.PipSize;
                            takeProfit = currentTakeProfit + ExtTrailPoint * Symbol.PipSize;
                        }
                        else
                        {
                            // If not trailing, set the stop loss to the opening price and adjust take profit
                            stopLoss = openPrice;
                            takeProfit = currentTakeProfit + ExtTrailPoint * Symbol.PipSize;
                        }

                        // Check if the Bid price exceeds the new stop loss level and modify the position accordingly
                        if (Symbol.Bid > stopLoss)
                        {
                            ModifyPosition(position, stopLoss, takeProfit);
                        }
                    }
                    // Check if the position is a Sell position
                    else if (type == TradeType.Sell)
                    {
                        // Determine whether to trail the stop loss based on the current stop loss relative to the opening price
                        if (currentStopLoss <= openPrice)
                        {
                            // Calculate the new stop loss and take profit levels with trailing applied
                            stopLoss = currentStopLoss - 2 * ExtTrailPoint * Symbol.PipSize;
                            takeProfit = currentTakeProfit - ExtTrailPoint * Symbol.PipSize;
                        }
                        else
                        {
                            // If not trailing, set the stop loss to the opening price and adjust take profit
                            stopLoss = openPrice;
                            takeProfit = currentTakeProfit - ExtTrailPoint * Symbol.PipSize;
                        }

                        // Check if the Ask price is below the new stop loss level and modify the position accordingly
                        if (Symbol.Ask < stopLoss)
                        {
                            ModifyPosition(position, stopLoss, takeProfit);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// This interface defines the contract for setting up and evaluating conditions for opening sell and buy positions based on candlestick data.
        /// Implement this interface to customize the setup and conditions for a trading strategy.
        /// </summary>
        public interface ISetup
    {
        /// <summary>
        /// Setup method is responsible for initializing any necessary parameters or settings for the trading strategy.
        /// It takes a 'Candles' object representing historical candlestick data and a 'bool' indicating whether trading is allowed or not.
        /// </summary>
        /// <param name="candles">The historical candlestick data used for analysis and decision-making.</param>
        /// <param name="isAllowed">A flag indicating whether trading is allowed or not based on external factors or constraints.</param>
        void Setup(Candles candles, bool isAllowed);

        /// <summary>
        /// Evaluate conditions to determine whether the strategy should open a sell position.
        /// </summary>
        /// <param name="candles">The historical candlestick data used for analysis.</param>
        /// <returns>True if the conditions for opening a sell position are met, otherwise false.</returns>
        bool OpenSellPositionConditions(Candles candles);

        /// <summary>
        /// Evaluate conditions to determine whether the strategy should open a buy position.
        /// </summary>
        /// <param name="candles">The historical candlestick data used for analysis.</param>
        /// <returns>True if the conditions for opening a buy position are met, otherwise false.</returns>
        bool OpenBuyPositionConditions(Candles candles);
    }


    /// <summary>
    /// The `A` class represents a trading strategy that can be used within the `Algo` trading algorithm.
    /// It defines conditions and actions for opening positions based on candlestick data and setup configurations.
    /// </summary>
    public class A : ISetup
    {
        private Algo Algo { get; set; }  // Reference to the parent `Algo` trading algorithm.

        /// <summary>
        /// Initializes an instance of the `A` class with a reference to the parent `Algo` trading algorithm.
        /// </summary>
        /// <param name="algo">The parent `Algo` trading algorithm.</param>
        public A(Algo algo)
        {
            Algo = algo;
        }

        /// <summary>
        /// This method defines the setup conditions for the `A` trading strategy and executes the setup if allowed.
        /// </summary>
        /// <param name="candles">A collection of candlestick data.</param>
        /// <param name="isAllowed">A flag indicating whether the setup is allowed.</param>
        public void Setup(Candles candles, bool isAllowed)
        {
            if (!isAllowed)
            {
                return;
            }

            // Check if both the Weekly (W1) and Daily (D1) candle directions are Negative.
            if (candles.W1.Direction == Candle.EDirection.Negative &&
                candles.D1.Direction == Candle.EDirection.Negative)
            {
                // If conditions for opening a sell position are met, proceed.
                if (OpenSellPositionConditions(candles))
                {
                    // Create a `PositionManager` to manage the position and set a sell position asynchronously.
                    PositionManager positionManager = new(Algo);
                    positionManager.SetSellPositionAsync();
                }
            }
            // Check if both the Weekly (W1) and Daily (D1) candle directions are Positive.
            else if (candles.W1.Direction == Candle.EDirection.Positive &&
                     candles.D1.Direction == Candle.EDirection.Positive)
            {
                // If conditions for opening a buy position are met, proceed.
                if (OpenBuyPositionConditions(candles))
                {
                    // Create a `PositionManager` to manage the position and set a buy position asynchronously.
                    PositionManager positionManager = new(Algo);
                    positionManager.SetBuyPositionAsync();
                }
            }
        }

        /// <summary>
        /// Defines the conditions for opening a sell position based on candlestick data.
        /// </summary>
        /// <param name="candles">A collection of candlestick data.</param>
        /// <returns>True if the conditions for opening a sell position are met; otherwise, false.</returns>
        public bool OpenSellPositionConditions(Candles candles)
        {
            PullBack pullBack = new();

            // Check for a medium moving average (MA) pullback for sell trades.
            if (pullBack.MediumMA(TradeType.Sell))
            {
                // Check if the conditions for closing a short fast moving average (MA) position are met.
                if (candles.Medium.CloseShortFastMA)
                {
                    // Check if the Slow MA direction is Negative or the conditions for closing a short Slow MA position are met.
                    if (candles.Slow.Direction == Candle.EDirection.Negative || candles.Slow.CloseShortFastMA)
                    {
                        // Check for the order of moving averages (Fast > Medium > Slow) to open a sell position.
                        if (MovingAverages.Slow[0] > MovingAverages.Medium[0] && MovingAverages.Medium[0] > MovingAverages.Fast[0])
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Defines the conditions for opening a buy position based on candlestick data.
        /// </summary>
        /// <param name="candles">A collection of candlestick data.</param>
        /// <returns>True if the conditions for opening a buy position are met; otherwise, false.</returns>
        public bool OpenBuyPositionConditions(Candles candles)
        {
            PullBack pullBack = new();

            // Check for a medium moving average (MA) pullback for buy trades.
            if (pullBack.MediumMA(TradeType.Buy))
            {
                // Check if the conditions for closing a long fast moving average (MA) position are met.
                if (candles.Medium.CloseLongFastMA)
                {
                    // Check if the Slow MA direction is Positive or the conditions for closing a long Slow MA position are met.
                    if (candles.Slow.Direction == Candle.EDirection.Positive || candles.Slow.CloseLongFastMA)
                    {
                        // Check for the order of moving averages (Fast < Medium < Slow) to open a buy position.
                        if (MovingAverages.Slow[0] < MovingAverages.Medium[0] && MovingAverages.Medium[0] < MovingAverages.Fast[0])
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Represents a strategy component B1 that implements the ISetup interface for setting up trading positions based on certain conditions.
    /// </summary>
    public class B1 : ISetup
    {
        private Algo Algo { get; set; }

        /// <summary>
        /// Initializes an instance of the B1 strategy component with a reference to the parent `Algo` trading algorithm.
        /// </summary>
        /// <param name="algo">The parent `Algo` trading algorithm.</param>
        public B1(Algo algo)
        {
            Algo = algo;
        }

        /// <summary>
        /// Sets up trading positions based on the provided candles data and permission status.
        /// </summary>
        /// <param name="candles">The candle data used for analysis.</param>
        /// <param name="isAllowed">A boolean indicating whether the setup is allowed.</param>
        public void Setup(Candles candles, bool isAllowed)
        {
            if (!isAllowed)
            {
                return;
            }

            // Check the direction of the W1 candle.
            if (candles.W1.Direction == Candle.EDirection.Negative)
            {
                if (OpenSellPositionConditions(candles))
                {
                    // Create a PositionManager instance and initiate a sell position.
                    PositionManager positionManager = new(Algo);
                    positionManager.SetBuyPositionAsync();
                }
            }
            else if (candles.W1.Direction == Candle.EDirection.Positive)
            {
                if (OpenBuyPositionConditions(candles))
                {
                    // Create a PositionManager instance and initiate a buy position.
                    PositionManager positionManager = new(Algo);
                    positionManager.SetBuyPositionAsync();
                }
            }
        }

        /// <summary>
        /// Determines whether the conditions for opening a sell position are met based on the provided candles data.
        /// </summary>
        /// <param name="candles">The candle data used for analysis.</param>
        /// <returns>True if the conditions for opening a sell position are met; otherwise, false.</returns>
        public bool OpenSellPositionConditions(Candles candles)
        {
            PullBack pullBack = new();

            if (pullBack.MediumMA(TradeType.Sell))
            {
                if (candles.Medium.CloseShortFastMA)
                {
                    if (candles.Slow.Direction == Candle.EDirection.Negative || candles.Slow.CloseShortFastMA)
                    {
                        if (MovingAverages.Slow[0] > MovingAverages.Medium[0] && MovingAverages.Medium[0] > MovingAverages.Fast[0])
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the conditions for opening a buy position are met based on the provided candles data.
        /// </summary>
        /// <param name="candles">The candle data used for analysis.</param>
        /// <returns>True if the conditions for opening a buy position are met; otherwise, false.</returns>
        public bool OpenBuyPositionConditions(Candles candles)
        {
            PullBack pullBack = new();

            if (pullBack.MediumMA(TradeType.Buy))
            {
                if (candles.Medium.CloseLongFastMA)
                {
                    if (candles.Slow.Direction == Candle.EDirection.Positive || candles.Slow.CloseLongFastMA)
                    {
                        if (MovingAverages.Slow[0] < MovingAverages.Medium[0] && MovingAverages.Medium[0] < MovingAverages.Fast[0])
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Represents a strategy component B2 that implements the ISetup interface for setting up trading positions based on certain conditions.
    /// </summary>
    public class B2 : ISetup
    {
        private Algo Algo { get; set; }

        /// <summary>
        /// Initializes an instance of the B2 strategy component with a reference to the parent `Algo` trading algorithm.
        /// </summary>
        /// <param name="algo">The parent `Algo` trading algorithm.</param>
        public B2(Algo algo)
        {
            Algo = algo;
        }

        /// <summary>
        /// Sets up trading positions based on the provided candles data and permission status.
        /// </summary>
        /// <param name="candles">The candle data used for analysis.</param>
        /// <param name="isAllowed">A boolean indicating whether the setup is allowed.</param>
        public void Setup(Candles candles, bool isAllowed)
        {
            if (!isAllowed)
            {
                return;
            }

            // Check the direction of the W1 and D1 candles.
            if (candles.W1.Direction == Candle.EDirection.Negative &&
                candles.D1.Direction == Candle.EDirection.Negative)
            {
                if (OpenSellPositionConditions(candles))
                {
                    // Create a PositionManager instance and initiate a sell position.
                    PositionManager positionManager = new(Algo);
                    positionManager.SetBuyPositionAsync();
                }
            }
            else if (candles.W1.Direction == Candle.EDirection.Positive &&
                     candles.D1.Direction == Candle.EDirection.Positive)
            {
                if (OpenBuyPositionConditions(candles))
                {
                    // Create a PositionManager instance and initiate a buy position.
                    PositionManager positionManager = new(Algo);
                    positionManager.SetBuyPositionAsync();
                }
            }
        }

        /// <summary>
        /// Determines whether the conditions for opening a sell position are met based on the provided candles data.
        /// </summary>
        /// <param name="candles">The candle data used for analysis.</param>
        /// <returns>True if the conditions for opening a sell position are met; otherwise, false.</returns>
        public bool OpenSellPositionConditions(Candles candles)
        {
            PullBack pullBack = new();

            if (pullBack.SlowMA(TradeType.Sell))
            {
                if (candles.Medium.CloseShortFastMA)
                {
                    if (candles.Slow.Direction == Candle.EDirection.Negative || candles.Slow.CloseShortFastMA)
                    {
                        if (MovingAverages.Slow[0] > MovingAverages.Medium[0] && MovingAverages.Medium[0] > MovingAverages.Fast[0])
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the conditions for opening a buy position are met based on the provided candles data.
        /// </summary>
        /// <param name="candles">The candle data used for analysis.</param>
        /// <returns>True if the conditions for opening a buy position are met; otherwise, false.</returns>
        public bool OpenBuyPositionConditions(Candles candles)
        {
            PullBack pullBack = new();

            if (pullBack.SlowMA(TradeType.Buy))
            {
                if (candles.Medium.CloseLongFastMA)
                {
                    if (candles.Slow.Direction == Candle.EDirection.Positive || candles.Slow.CloseLongFastMA)
                    {
                        if (MovingAverages.Slow[0] < MovingAverages.Medium[0] && MovingAverages.Medium[0] < MovingAverages.Fast[0])
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Represents a strategy component C that implements the ISetup interface for setting up trading positions based on certain conditions.
    /// </summary>
    public class C : ISetup
    {
        private Algo Algo { get; set; }

        /// <summary>
        /// Initializes an instance of the C strategy component with a reference to the parent `Algo` trading algorithm.
        /// </summary>
        /// <param name="algo">The parent `Algo` trading algorithm.</param>
        public C(Algo algo)
        {
            Algo = algo;
        }

        /// <summary>
        /// Sets up trading positions based on the provided candles data and permission status.
        /// </summary>
        /// <param name="candles">The candle data used for analysis.</param>
        /// <param name="isAllowed">A boolean indicating whether the setup is allowed.</param>
        public void Setup(Candles candles, bool isAllowed)
        {
            if (!isAllowed)
            {
                return;
            }

            // Check if conditions for opening a sell position are met.
            if (OpenSellPositionConditions(candles))
            {
                // Create a PositionManager instance and initiate a sell position.
                PositionManager positionManager = new(Algo);
                positionManager.SetBuyPositionAsync();
            }
            // Check if conditions for opening a buy position are met.
            else if (OpenBuyPositionConditions(candles))
            {
                // Create a PositionManager instance and initiate a buy position.
                PositionManager positionManager = new(Algo);
                positionManager.SetBuyPositionAsync();
            }
        }

        /// <summary>
        /// Determines whether the conditions for opening a sell position are met based on the provided candles data.
        /// </summary>
        /// <param name="candles">The candle data used for analysis.</param>
        /// <returns>True if the conditions for opening a sell position are met; otherwise, false.</returns>
        public bool OpenSellPositionConditions(Candles candles)
        {
            PullBack pullBack = new();

            if (pullBack.FastMA(TradeType.Sell))
            {
                if (candles.Medium.CloseShortFastMA)
                {
                    if (candles.Slow.Direction == Candle.EDirection.Negative || candles.Slow.CloseShortFastMA)
                    {
                        if (MovingAverages.Slow[0] > MovingAverages.Medium[0] && MovingAverages.Medium[0] > MovingAverages.Fast[0])
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the conditions for opening a buy position are met based on the provided candles data.
        /// </summary>
        /// <param name="candles">The candle data used for analysis.</param>
        /// <returns>True if the conditions for opening a buy position are met; otherwise, false.</returns>
        public bool OpenBuyPositionConditions(Candles candles)
        {
            PullBack pullBack = new();

            if (pullBack.FastMA(TradeType.Buy))
            {
                if (candles.Medium.CloseLongFastMA)
                {
                    if (candles.Slow.Direction == Candle.EDirection.Positive || candles.Slow.CloseLongFastMA)
                    {
                        if (MovingAverages.Slow[0] < MovingAverages.Medium[0] && MovingAverages.Medium[0] < MovingAverages.Fast[0])
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }

    /// <summary>
    /// Represents a class for evaluating pullback conditions based on moving averages and candlestick data.
    /// </summary>
    public class PullBack : WeeklyDailyEMA
    {
        /// <summary>
        /// Evaluates pullback conditions based on the fast moving average for a specified trade type.
        /// </summary>
        /// <param name="tradeType">The trade type, either Buy or Sell.</param>
        /// <returns>True if pullback conditions are met; otherwise, false.</returns>
        public bool FastMA(TradeType tradeType)
        {
            Bars bars = MarketData.GetBars(TimeFrame, Symbol.Name);
            double thresHold = ExtFastThresHold * Symbol.PipSize / Symbol.Ask * Symbol.LotSize;
            int shift = 10;

            switch (tradeType)
            {
                case TradeType.Sell:
                    for (int i = 1; i < shift; i++)
                    {
                        if (bars.Last(i).High >= MovingAverages.Fast[i] - thresHold)
                        {
                            if (MovingAverages.Slow[i] > MovingAverages.Medium[i] && MovingAverages.Medium[i] > MovingAverages.Fast[i])
                            {
                                return true;
                            }
                        }
                    }
                    break;
                case TradeType.Buy:
                    for (int i = 1; i < shift; i++)
                    {
                        if (bars.Last(i).Low <= MovingAverages.Fast[i] + thresHold)
                        {
                            if (MovingAverages.Slow[i] > MovingAverages.Medium[i] && MovingAverages.Medium[i] > MovingAverages.Fast[i])
                            {
                                return true;
                            }
                        }
                    }
                    break;
            }

            return false;
        }

        /// <summary>
        /// Evaluates pullback conditions based on the medium moving average for a specified trade type.
        /// </summary>
        /// <param name="tradeType">The trade type, either Buy or Sell.</param>
        /// <returns>True if pullback conditions are met; otherwise, false.</returns>
        public bool MediumMA(TradeType tradeType)
        {
            Bars bars = MarketData.GetBars(TimeFrame, Symbol.Name);
            double thresHold = ExtFastThresHold * Symbol.PipSize / Symbol.Ask * Symbol.LotSize;
            int shift = 10;

            switch (tradeType)
            {
                case TradeType.Sell:
                    for (int i = 1; i < shift; i++)
                    {
                        if (bars.Last(i).High >= MovingAverages.Medium[i] - thresHold)
                        {
                            if (MovingAverages.Slow[i] > MovingAverages.Medium[i] && MovingAverages.Medium[i] > MovingAverages.Fast[i])
                            {
                                return true;
                            }
                        }
                    }
                    break;
                case TradeType.Buy:
                    for (int i = 1; i < shift; i++)
                    {
                        if (bars.Last(i).Low <= MovingAverages.Medium[i] + thresHold)
                        {
                            if (MovingAverages.Slow[i] > MovingAverages.Medium[i] && MovingAverages.Medium[i] > MovingAverages.Fast[i])
                            {
                                return true;
                            }
                        }
                    }
                    break;
            }

            return false;
        }

        /// <summary>
        /// Evaluates pullback conditions based on the slow moving average for a specified trade type.
        /// </summary>
        /// <param name="tradeType">The trade type, either Buy or Sell.</param>
        /// <returns>True if pullback conditions are met; otherwise, false.</returns>
        public bool SlowMA(TradeType tradeType)
        {
            Bars bars = MarketData.GetBars(TimeFrame, Symbol.Name);
            double thresHold = ExtFastThresHold * Symbol.PipSize / Symbol.Ask * Symbol.LotSize;
            int shift = 10;

            switch (tradeType)
            {
                case TradeType.Sell:
                    for (int i = 1; i < shift; i++)
                    {
                        if (bars.Last(i).High >= MovingAverages.Slow[i] - thresHold)
                        {
                            if (MovingAverages.Slow[i] > MovingAverages.Medium[i] && MovingAverages.Medium[i] > MovingAverages.Fast[i])
                            {
                                return true;
                            }
                        }
                    }
                    break;
                case TradeType.Buy:
                    for (int i = 1; i < shift; i++)
                    {
                        if (bars.Last(i).Low <= MovingAverages.Slow[i] + thresHold)
                        {
                            if (MovingAverages.Slow[i] > MovingAverages.Medium[i] && MovingAverages.Medium[i] > MovingAverages.Fast[i])
                            {
                                return true;
                            }
                        }
                    }
                    break;
            }

            return false;
        }
    }


    /// <summary>
    /// The `PositionManager` class is responsible for managing trading positions and orders within the trading algorithm (`Algo`).
    /// It handles the placement and cancellation of buy and sell orders asynchronously.
    /// </summary>
    public class PositionManager : Robot
    {
        private Algo Algo { get; set; }
        private double Volume { get; set; }

        private int _numberOfPendingPlaceOrderOperations;     // Number of pending order placement operations.
        private int _numberOfPendingCancelOrderOperations;    // Number of pending order cancellation operations.

        private readonly int amountOfOrders = 2;   // The number of orders to place.

        /// <summary>
        /// Initializes an instance of the `PositionManager` class with a reference to the parent `Algo` trading algorithm and a specified trading volume.
        /// </summary>
        /// <param name="algo">The parent `Algo` trading algorithm.</param>
        /// <param name="volume">The trading volume for the positions managed by this instance.</param>
        public PositionManager(Algo algo, double volume = 0.01)
        {
            Algo = algo;

            // Ensure that the specified trading volume is valid and greater than or equal to the minimum allowed volume.
            if (volume < 0 || volume < Symbol.VolumeInUnitsMin)
            {
                Volume = Symbol.VolumeInUnitsMin;
            }
            else
            {
                Volume = volume;
            }
        }

        /// <summary>
        /// Asynchronously sets buy positions by placing limit orders for a specified number of orders.
        /// </summary>
        public void SetBuyPositionAsync()
        {
            for (int i = 0; i < amountOfOrders; i++)
            {
                Interlocked.Increment(ref _numberOfPendingPlaceOrderOperations);

                // Place a limit buy order with specific parameters and callback function.
                PlaceLimitOrderAsync(TradeType.Buy,
                                       SymbolName,
                                       Volume,
                                       Symbol.Ask - (Symbol.PipSize * 5),
                                       "Trade_" + i,
                                       OnOrderPlaced);
            }
        }

        /// <summary>
        /// Asynchronously sets sell positions by placing limit orders for a specified number of orders.
        /// </summary>
        public void SetSellPositionAsync()
        {
            for (int i = 0; i < amountOfOrders; i++)
            {
                Interlocked.Increment(ref _numberOfPendingPlaceOrderOperations);

                // Place a limit sell order with specific parameters and callback function.
                PlaceLimitOrderAsync(TradeType.Buy,
                                       SymbolName,
                                       Volume,
                                       Symbol.Bid + (Symbol.PipSize * 5),
                                       "Trade_" + i,
                                       OnOrderPlaced);
            }
        }

        /// <summary>
        /// Callback method executed when an order is successfully placed.
        /// </summary>
        /// <param name="result">The result of the order placement operation.</param>
        private void OnOrderPlaced(TradeResult result)
        {
            if (Interlocked.Decrement(ref _numberOfPendingPlaceOrderOperations) == 0)
            {
                Algo.Print("All orders have been placed.");
            }
        }

        /// <summary>
        /// Asynchronously cancels all pending orders with labels containing "Trade_".
        /// </summary>
        private void CancelAllPendingOrdersAsync()
        {
            var pendingOrders = Algo.PendingOrders.Where(o => o.Label.Contains("Trade_")).ToArray();

            foreach (var order in pendingOrders)
            {
                Interlocked.Increment(ref _numberOfPendingCancelOrderOperations);

                // Cancel a pending order with a specific callback function.
                CancelPendingOrderAsync(order, OnOrderCancel);
            }
        }

        /// <summary>
        /// Callback method executed when an order is successfully canceled.
        /// </summary>
        /// <param name="result">The result of the order cancellation operation.</param>
        private void OnOrderCancel(TradeResult result)
        {
            if (Interlocked.Decrement(ref _numberOfPendingCancelOrderOperations) == 0)
            {
                Algo.Print("All orders have been canceled.");
            }
        }
    }

}}

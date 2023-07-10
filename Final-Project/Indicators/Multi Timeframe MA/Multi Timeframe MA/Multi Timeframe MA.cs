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
    [Indicator(IsOverlay = true, TimeZone = TimeZones.UTC)]
    public class MultiTimeframeMA : Indicator
    {
        [Parameter("Source")]
        public DataSeries Source { get; set; }
        
        [Parameter(DefaultValue = 800)]
        public int FastPeriod { get; set; }
        [Parameter(DefaultValue = 200)]
        public int MediumPeriod { get; set; }
        [Parameter(DefaultValue = 50)]
        public int SlowPeriod { get; set; }
        
        [Output("Fast MA", LineColor = "#FF0000")]
        public IndicatorDataSeries FastMA { get; set; }
        
        [Output("Meduium MA", LineColor = "#008000")]
        public IndicatorDataSeries MediumMA { get; set; }
        
        [Output("Slow MA", LineColor = "#7EC8E3")]
        public IndicatorDataSeries SlowMA { get; set; }

        [Parameter("MA type", DefaultValue = MovingAverageType.Simple)]
        public MovingAverageType MAType { get; set; }

        private MovingAverage _fastMA;
        private MovingAverage _mediumMA;
        private MovingAverage _slowMA;
 
        protected override void Initialize()
        {
            _fastMA = Indicators.MovingAverage(Source, FastPeriod, MAType);
            _mediumMA = Indicators.MovingAverage(Source, MediumPeriod, MAType);
            _slowMA = Indicators.MovingAverage(Source, SlowPeriod, MAType);
        }
 
        public override void Calculate(int index)
        {
            FastMA[index] = _fastMA.Result[index];
            MediumMA[index] = _mediumMA.Result[index];
            SlowMA[index] = _slowMA.Result[index];
        }
    }
}
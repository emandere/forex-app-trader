using System;
namespace forex_app_trader.Domain
{
    public  class ForexIndicator
    {
        public string Pair {get;set;}
        public string StartDate { get; set; }
        public string EndDate { get; set; }   
        public string IndicatorDisplay { get; set; }

        public double Indicator{ get; set;}
    }   
}
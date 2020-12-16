using System;
namespace forex_app_trader.Domain
{
    public  class ForexPrice
    {
        public string Instrument { get; set; }
        public string Time { get; set; }   
        public double Bid { get; set; }
        public double Ask { get; set; }
    }   
}
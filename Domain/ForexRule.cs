using System;
namespace forex_app_trader.Domain
{
    public  class ForexRule
    {
        public string RuleName { get; set; }
        public bool IsMet { get; set; }   
        public int window { get; set; }
    }   
}
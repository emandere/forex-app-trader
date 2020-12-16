using System.Collections.Generic;
using System.Linq;
using forex_app_trader.Domain.Indicators;
namespace forex_app_trader.Domain.Rules
{
    public class RSIOverbought70:IRule
    {
        public string Indicator() => "RSI";
        public bool IsMet(IEnumerable<ForexDailyPrice> window)
        {
            if(Stats.RSI(window.Select(z=> new List<double>{z.Open,z.Close})) > 70 )
                return true;
            else
                return false;
        }
    }
}

using System.Collections.Generic;
using forex_app_trader.Domain;
namespace forex_app_trader.Domain.Rules
{
    public interface IRule
    {
        bool IsMet(IEnumerable<ForexDailyPrice> window);
    }
}
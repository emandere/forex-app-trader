using System;
using System.Collections.Generic;
using System.Linq;

namespace forex_app_trader.Domain.Indicators
{
    public class Stats
    {
        public static double Average(IEnumerable<double> x)
        {
            double sum = x.Aggregate((t,e)=>t+e);
            double xavg = sum / x.Count();
            return xavg;
        }

        public static double Average(IEnumerable<double> x,int y)
        {
            double sum = x.Aggregate((t,e)=>t+e);
            double xavg = sum / y;
            return xavg;
        }

        public static double StdDev(IEnumerable<double> x)
        {
           double sumsquared = x.Select(t=>t*t).Aggregate((t,e)=>t+e);
           double stdDev = Math.Sqrt((sumsquared/x.Count()) - Average(x)*Average(x));
           return stdDev;   
        }

        public static double BollingerUpper(List<double> x)
        {
            double val = x.Last();
            return val + 2* StdDev(x);
        }
        public static double BollingerLower(List<double> x)
        {
            double bollinger = 999.0;

            if(x.Count() > 0)
            {
                double val = x.Last();
                bollinger = val - 2* StdDev(x);
            }

            return bollinger;
        }

        public static double RSI(IEnumerable<List<double>> x)
        {
            var diff = x.Select((t)=>t[1]-t[0]);
            var gains = diff.Where((t)=>t>0);
            var losses = diff.Where((t)=>t<0);
            double RS = 100.0;

            if(losses.Count()>0)
                RS = Math.Abs(Average(gains)/Average(losses));

            double RSI = 100 - (100/(1+RS));

            return RSI;
        }

        public static double RSIReal(IEnumerable<List<double>> x)
        {
            var diff = x.Select((t)=>t[1]-t[0]);
            var gains = diff.Where((t)=>t>0);
            var losses = diff.Where((t)=>t<0);
            double RS = 100.0;

            if(losses.Count()>0)
                RS = Math.Abs(Average(gains,x.Count())/Average(losses,x.Count()));

            double RSI = 100 - (100/(1+RS));

            return RSI;
        }


    }
}
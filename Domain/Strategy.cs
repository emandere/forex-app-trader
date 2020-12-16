namespace forex_app_trader.Domain
{
   public class Strategy
   {
        public int window{get;set;}
        public string ruleName{get;set;}
        public string position{get;set;}
        public int units{get;set;}
        public double stopLoss{get;set;}
        public double takeProfit{get;set;}

   } 
}
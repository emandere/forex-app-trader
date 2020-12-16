using System.Collections.Generic;
using System.Linq;
using System;

namespace forex_app_trader.Domain
{
    public  class ForexSession
    {
        public bool ExecuteTrade(string pair,double price,int units,double stopLoss,double takeProfit,bool position,string date)
        {
            Trade trade = new Trade();
            trade.Id = this.SessionUser.Accounts.Primary.Trades.Count;
            trade.Pair=pair;
            trade.Units=units;
            trade.OpenPrice = price;
            trade.ClosePrice = price;
            trade.StopLoss = stopLoss;
            trade.TakeProfit = takeProfit;
            trade.Long = position;
            trade.OpenDate = date;
            this.SessionUser.Accounts.Primary.Trades.Add(trade);
            return true;
        }

        public bool UpdateSession(string pair,double bid,double ask,string closeDate)
        {
            List<Trade> trades = this.SessionUser.Accounts.Primary.Trades.Where(x => x.Pair==pair).ToList();
            foreach(var trade in trades)
            {
                trade.CloseDate = closeDate;
                if(trade.Long)
                {
                    trade.ClosePrice = ask;
                    if( (trade.ClosePrice > trade.TakeProfit) ||
                        (trade.ClosePrice < trade.StopLoss))
                    {
                        closeTrade(trade);
                    }
                }
                else
                {
                    trade.ClosePrice = bid;
                    if( (trade.ClosePrice < trade.TakeProfit) ||
                        (trade.ClosePrice > trade.StopLoss))
                    {
                        closeTrade(trade);
                    }
                }
               
            }

            string tradeDay = DateTime.Parse(closeDate).ToString("yyyy-MM-dd");
            this.CurrentTime = tradeDay;
            var history = new BalanceHistory
            {
                Date = tradeDay,
                Amount = this.SessionUser.Accounts.Primary.NetAssetValue
            };
            this.SessionUser.Accounts.Primary.UpdateHistory(history);

            return true;
        }

        private void closeTrade(Trade trade)
        {
            
            this.SessionUser.Accounts.Primary.Cash +=trade.PL;
            this.SessionUser.Accounts.Primary.ClosedTrades.Add(trade);
            this.SessionUser.Accounts.Primary.Trades.Remove(trade);

        }

        /*public Account GetAccountByPair(string pair)
        {
            Account acc = new Account();
            SortedSet<string> setSessionDates = new SortedSet<string>();
            SortedSet<string> setCloseDates = new SortedSet<string>();
            List<BalanceHistory> hist = new List<BalanceHistory>();
            double pairAmount = SessionUser.Accounts
                                            .Primary
                                            .BalanceHistory[0]
                                            .Amount;
            
            acc.ClosedTrades = SessionUser.Accounts
                                            .Primary
                                            .ClosedTrades
                                            .Where(x=>x.Pair==pair)
                                            .ToList();
                                            

            foreach(var history in SessionUser.Accounts.Primary.BalanceHistory)
            {                               
                setSessionDates.Add(history.Date);
            }

            foreach(Trade closedTrade in  acc.ClosedTrades)
            {
                setCloseDates.Add(DateTime.Parse(closedTrade.CloseDate).ToString("yyyy-MM-dd"));
            }

            
            foreach(string sessdate in setSessionDates)
            {
                if(setCloseDates.Contains(sessdate))
                {
                    pairAmount+=acc.ClosedTrades.Where(x=>DateTime.Parse(x.CloseDate).ToString("yyyy-MM-dd")==sessdate)
                                                .Select(x=>x.PL)
                                                .Sum();
                }
                hist.Add(new BalanceHistory(){Date=sessdate,Amount=pairAmount});
            }
            
            acc.BalanceHistory = hist;

            return acc;
        }*/
        public string Id { get; set; }
        public string idinfo { get; set; }

        public string SessionType { get; set; }

        public string ExperimentId { get; set; }

        public string StartDate { get; set; }

       
        public string EndDate { get; set; }

        
        public string LastUpdatedTime { get; set; }

        
        public string CurrentTime { get; set; }

        public double RealizedPL{get=>Math.Round(SessionUser.RealizedPL,2);}

        public Strategy Strategy { get; set; }

        public SessionUser SessionUser { get; set; }

        public double Balance { get => Math.Round(SessionUser.Balance,2);}

        public string PercentComplete { get; set; }

        public string elapsedTime { get; set; }
        public string beginSessionTime { get; set; }
        public string endSessionTime { get; set; }
    }

    public  class SessionUser
    {
        
        public string Id { get; set; }

        public string idinfo { get; set; }

        public object Status { get; set; }
        public Accounts Accounts { get; set; }

        public double Balance 
        {
            get => Accounts.Primary.NetAssetValue;
                
        }

        public int ClosedTrades
        {
            get => Accounts.Primary.ClosedTrades.Count();
        }

        public int OpenTrades
        {
            get => Accounts.Primary.Trades.Count();
        }

        public double PercentProfitableClosed
        {
            get => ClosedTrades > 0 ? Math.Round(PercentProfitableClosedCount(),2): 0;
        }

        private double PercentProfitableClosedCount()
        {
            return (Accounts.Primary.ClosedTrades.Where(x=>x.PLCalc() > 0).Count() / (double)ClosedTrades) * 100.0;
        }

        public double PercentProfitableOpen
        {
            get => OpenTrades > 0 ? Math.Round(PercentProfitableOpenCount(),2): 0;
        }

        private double PercentProfitableOpenCount()
        {
            return (Accounts.Primary.Trades.Where(x=>x.PLCalc() > 0).Count() / (double)OpenTrades) * 100.0;
        }

        public double RealizedPL 
        {
            get => Accounts.Primary.RealizedPL;
        }

        
    }

    public  class Accounts
    {
        
        public Account Primary { get; set; }

        public Account Secondary { get; set; }
    }

    public  class Account
    {
        private double _realizedPL;

        private List<BalanceHistory> _history;

        private double _cash;

       
        public string Id { get; set; }

        public string idinfo { get; set; }

        public double Cash 
        { 
            get=>_cash;
            set=> _cash=value;
        }

        public long Margin { get; set; }

       
        public long MarginRatio { get; set; }

      
        public double RealizedPL 
        { 
            get => BalanceHistory.Count > 0 ? BalanceHistory.Last().Amount - BalanceHistory.First().Amount : 0;
        }

        public double NetAssetValue
        {
            get => Cash + UnRealizedPL;
        }


        public double UnRealizedPL
        {
            get => Trades.Sum(x => x.PL);
        }

      
        public List<Trade> Trades { get; set; }

      
        public Order[] Orders { get; set; }

      
        public List<Trade> ClosedTrades { get; set; }

     
        public List<BalanceHistory> BalanceHistory { get; set; }

        public void UpdateHistory(BalanceHistory history)
        {
            var currHistory = BalanceHistory.FirstOrDefault(x => x.Date == history.Date);
            if(currHistory!=null)
                BalanceHistory.Remove(currHistory);

            BalanceHistory.Add(history);
        }

     
        public long Idcount { get; set; }
    }


    public  class BalanceHistory
    {
     
        public string Date { get; set; }

     
        public double Amount { get; set; }
    }

    public  class Trade
    {
        public long? Id { get; set; }

     
        public long? idinfo { get; set; }

     
        public string Pair { get; set; }

       
        public long Units { get; set; }

      
        public string OpenDate { get; set; }

      
        public string CloseDate { get; set; }

     
        public bool Long { get; set; }

       
        public double OpenPrice { get; set; }

      
        public double ClosePrice { get; set; }

        public double? StopLoss { get; set; }

        public double? TakeProfit { get; set; }


        public bool Init { get; set; }

        public double PL { get => PLCalc();}

        double Position()
        {
            if(Long)
                return 1.0;
            else
                return -1.0;
        }

        double adj()
        {
            double adj= (Pair=="USDJPY") ? 0.01 : 1.0;
            return adj;
        }
        public double PLCalc()
        {
            return Units * Position() * (ClosePrice - OpenPrice)*adj();
        }

        
    }

    public  class Order
    {
        public object ExpirationDate { get; set; }

        public double Triggerprice { get; set; }

        public bool Expired { get; set; }

        public bool Above { get; set; }

        public Trade Trade { get; set; }
    }

}

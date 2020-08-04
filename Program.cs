using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Linq;
using System.Text.Json;
using forex_app_trader.Models;
using Microsoft.Extensions.Configuration;

namespace forex_app_trader
{
    class Program
    {
        static readonly HttpClient client = new HttpClient();

        static List<string> pairs = new List<string>()
        {
            "AUDUSD",
            "EURUSD",
            "GBPUSD",
            "NZDUSD",
            "USDCAD",
            "USDCHF",
            "USDJPY"
        };

        static Dictionary<string,string> pairToInstrument= new Dictionary<string,string>()
        {
            {"AUDUSD","AUD_USD"},
            {"EURUSD","EUR_USD"},
            {"GBPUSD","GBP_USD"},
            {"NZDUSD","NZD_USD"},
            {"USDCAD","USD_CAD"},
            {"USDCHF","USD_CHF"},
            {"USDJPY","USD_JPY"},
        };


        static async Task Main(string[] args)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env}.json", optional: true, reloadOnChange: true);
            IConfigurationRoot configuration = builder.Build();
            string server = configuration.GetSection("Servers:Local").Value;
            //await runTestData(server);
            Console.WriteLine(server);
            await runDailyTrader(server);
        }
        

        static async Task<bool> ShouldExecuteTrade(string server,string pair, string ruleName,string currDay,int window)
        {
            string urlgetStrategy = $"http://{server}/api/forexrule/{ruleName}/{pair}/{currDay}/{window}";
            var ruleResult = await GetAsync<ForexRuleDTO>(urlgetStrategy);
            if(ruleResult.IsMet)
                return true;
            else
                return false;   
        }

        static async Task executeTrade(string server,ForexSessionDTO session,ForexPriceDTO currPrice,string currDay)
        {
            //string currDay = currPrice.UTCTime.ToString("yyyy-MM-dd");
            string urlpatchtrade = $"http://{server}/api/forexsession/executetrade/{session.Id}";
            string urlrealtrade = $"http://{server}/api/forextrade";

            var openTradesDTO =  await GetAsync<ForexOpenTradesDTO>(urlrealtrade);
            var openTradeUnits = openTradesDTO.Trades.Select(x => x.Units);//session.SessionUser.Accounts.Primary.Trades.Select(x => x.Units);
            var trade = new ForexTradeDTO()
            {
                Pair = currPrice.Instrument,
                Price = currPrice.Bid,
                Units = (int) getFiFo(openTradeUnits,session.Strategy.units),
                StopLoss = currPrice.Bid * session.Strategy.stopLoss,
                TakeProfit = currPrice.Bid * session.Strategy.takeProfit,
                Date = currDay
            };

            var realtrade = new ForexTradeDTO()
            {
                Pair = pairToInstrument[currPrice.Instrument],
                Price = currPrice.Bid,
                Units = (int) getFiFo(openTradeUnits,session.Strategy.units),
                StopLoss = currPrice.Bid * session.Strategy.stopLoss,
                TakeProfit = currPrice.Bid * session.Strategy.takeProfit,
                Date = currDay
            };
    
            var responseTradeBody =await PatchAsync<ForexTradeDTO>(trade,urlpatchtrade);
            var responseRealTradeBody =await PostAsync<ForexTradeDTO>(realtrade,urlrealtrade);
        }

        static long getFiFo(IEnumerable<long> units,int defaultUnits)
        {
            return units.Count() > 0 ? units.Select( x => Math.Abs(x)).Max() + 1 : defaultUnits;
        }

        static async Task runDailyTrader(string server)
        {
            string sessionName = "liveSessionRSICSharp";
            string urlget = $"http://{server}/api/forexsession/{sessionName}";
            string urlpost = $"http://{server}/api/forexsession";
            string urlpatchprice = $"http://{server}/api/forexsession/updatesession/{sessionName}";
            string urlgetdailyrealprices = $"http://{server}/api/forexprices";

            var sessionList = await GetAsync<ForexSessionsDTO>(urlget);
           

            if(sessionList.sessions.Length == 0)
            {
                var sessionIn = new ForexSessionDTO()
                {
                    Id = sessionName,
                    StartDate = DateTime.Now.ToString("yyyy-MM-dd"),
                    SessionType = "live",
                    SessionUser = new SessionUserDTO()
                    {
                        Accounts = new AccountsDTO()
                        {
                            Primary = new AccountDTO()
                            {
                                Id = "primary",
                                Cash = 191.41,
                            }
                        }

                    },
                    Strategy = new StrategyDTO()
                    {
                        ruleName = "RSI",
                        window = 15,
                        position = "short",
                        stopLoss = 1.007,
                        takeProfit = 0.998,
                        units = 100
                    }
                };

                var sessions = new ForexSessionsDTO()
                {
                    sessions = new ForexSessionDTO[]{sessionIn}
                };
                var responsePostBody = await PostAsync<ForexSessionsDTO>(sessions,urlpost);
            }
            
            while(true)
            { 
                try
                {
                    var sessionsDTO = await GetAsync<ForexSessionsDTO>(urlget);
                    var session = sessionsDTO.sessions[0];

                    var currDay = DateTime.Now.ToString("yyyy-MM-dd");
                    var currDayTrade =  DateTime.Now.ToString("yyyy-MM-dd");
                    
                    var prices = await GetAsync<ForexPricesDTO>(urlgetdailyrealprices);
                    foreach(var pair in pairs)
                    {
                        var days = session.SessionUser
                                        .Accounts
                                        .Primary
                                        .Trades
                                        .Where(x=>x.Pair==pair)
                                        .Select(x => x.OpenDate).ToList();

                        days.InsertRange
                        (0,session
                            .SessionUser
                            .Accounts
                            .Primary
                            .ClosedTrades
                            .Where(x=>x.Pair==pair)
                            .Select(x => x.OpenDate).ToList()
                        );
                        
                        var price = prices.prices.FirstOrDefault(x => x.Instrument == pair);
                        if(!days.Contains(currDayTrade))
                        {
                            bool shouldTrade = await ShouldExecuteTrade(server,pair,"RSI",currDay,14);
                            Console.WriteLine($"{pair} {price.Bid} {shouldTrade}");
                            if(shouldTrade)
                            {
                                await executeTrade(server,session,price,currDayTrade);
                                sessionList = await GetAsync<ForexSessionsDTO>(urlget);
                                session = sessionList.sessions[0];
                            }
                            
                        }
                        var responsePriceBody = await PatchAsync<ForexPriceDTO>(price,urlpatchprice);
                    }
                    Console.WriteLine("Updating");
                    await Task.Delay(1000*30*1);
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }


        static async Task runTestData(string server)
        {
            string sessionName = "liveSession2";
            string urlget = $"http://localhost:5002/api/forexsession/{sessionName}";
            string urlpost = $"http://localhost:5002/api/forexsession";
            string urlpatchprice = $"http://localhost:5002/api/forexsession/updatesession/{sessionName}";

            var startDate = "20190324";
            var endDate = "20200522";

            var sessionList = await GetAsync<ForexSessionsDTO>(urlget);

            if(sessionList.sessions.Length > 0)
                await client.DeleteAsync(urlget);
            
            var sessionIn = new ForexSessionDTO()
            {
                Id = sessionName,
                SessionType = "live",
                SessionUser = new SessionUserDTO()
                {
                     Accounts = new AccountsDTO()
                     {
                         Primary = new AccountDTO()
                         {
                             Id = "primary",
                             Cash = 191.41,
                         }
                     }

                },
                Strategy = new StrategyDTO()
                {
                    ruleName = "RSI",
                    window = 15,
                    position = "short",
                    stopLoss = 1.007,
                    takeProfit = 0.998,
                    units = 100
                }
            };

            //var sessionsList = new ForexSessionDTO[]{sessionIn};
            var sessions = new ForexSessionsDTO()
            {
                sessions = new ForexSessionDTO[]{sessionIn}
            };
            var responsePostBody = await PostAsync<ForexSessionsDTO>(sessions,urlpost);

            var sessionsDTO = await GetAsync<ForexSessionsDTO>(urlget);
            var session = sessionsDTO.sessions[0];

            var urlGetDailyPricesRange = $"http://localhost:5002/api/forexdailyprices/AUDUSD/{startDate}/{endDate}"; 
            var dailypricesRange = await GetAsync<List<ForexDailyPriceDTO>>(urlGetDailyPricesRange);   
            foreach(var dailyPrice in dailypricesRange)
            {
                foreach(var pair in pairs)
                {
                    var currDay = dailyPrice.Datetime.ToString("yyyy-MM-dd");
                    var currDayRealTime = dailyPrice.Datetime.ToString("yyyyMMdd");
                    var urlgetdailyrealprices = $"http://localhost:5002/api/forexdailyrealprices/{pair}/{currDayRealTime}";
                    
                    var dailyrealprices = await GetAsync<ForexPricesDTO>(urlgetdailyrealprices);
                    Console.WriteLine($"{pair} {currDay}");
                    bool shouldTrade = await ShouldExecuteTrade(server,pair,session.Strategy.ruleName,currDay,session.Strategy.window);
                    if(shouldTrade)
                    {
                        await executeTrade(server,session,dailyrealprices.prices[0],currDayRealTime);
                        sessionList = await GetAsync<ForexSessionsDTO>(urlget);
                        session = sessionList.sessions[0];
                    }
                    var tradepairs = session.SessionUser.Accounts.Primary.Trades.Select(x=>x.Pair);
                    if(tradepairs.Contains(pair))
                    {
                        foreach(var realPrice in dailyrealprices.prices.Take(100))
                        {
                            //Console.WriteLine($" {realPrice.Time} {realPrice.Bid}");
                            var responsePriceBody = await PatchAsync<ForexPriceDTO>(realPrice,urlpatchprice);
                        }
                        sessionList = await GetAsync<ForexSessionsDTO>(urlget);
                        session = sessionList.sessions[0];
                    }

                    
                }

            }
        }

      
        static async Task<T> GetAsync<T>(string url)
        {
            var responseBody = await client.GetStringAsync(url);
            var data = JsonSerializer.Deserialize<T>(responseBody);
            return data;
        }

        static async Task<HttpResponseMessage> PatchAsync<T>(T dto,string url)
        {
            var stringPrice= JsonSerializer.Serialize<T>(dto);
            var stringPriceContent = new StringContent(stringPrice,UnicodeEncoding.UTF8,"application/json");
            var responsePriceBody = await client.PatchAsync(url,stringPriceContent);
            return responsePriceBody;
        }

        static async Task<HttpResponseMessage> PostAsync<T>(T dto,string url)
        {
            var stringPrice= JsonSerializer.Serialize<T>(dto);
            var stringPriceContent = new StringContent(stringPrice,UnicodeEncoding.UTF8,"application/json");
            var responsePriceBody = await client.PostAsync(url,stringPriceContent);
            return responsePriceBody;
        }
    }
}

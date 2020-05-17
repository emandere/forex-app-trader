using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using forex_app_trader.Models;

namespace forex_app_trader
{
    class Program
    {
        static readonly HttpClient client = new HttpClient();
        static async Task Main(string[] args)
        {
            string sessionName = "liveSession2";
            string urlget = $"http://localhost:5002/api/forexsession/{sessionName}";
            string urlpost = $"http://localhost:5002/api/forexsession";
            string urlpatchtrade = $"http://localhost:5002/api/forexsession/executetrade/{sessionName}";
            string urlpatchprice = $"http://localhost:5002/api/forexsession/updatesession/{sessionName}";
            string urlgetRSI = $"http://localhost:5002/api/forexrule/RSI/AUDUSD/2020-01-01/15";
            
            var rsiResponse = await GetAsync<ForexRuleDTO>(urlgetRSI);
            var responseBody = await client.GetStringAsync(urlget);
            var sessionList = JsonSerializer.Deserialize<ForexSessionsDTO>(responseBody);

            if(sessionList.sessions.Length > 0)
                await client.DeleteAsync(urlget);
            
            ForexSessionInDTO session = new ForexSessionInDTO()
            {
                Id = sessionName,
                SessionUser = new SessionUserInDTO()
                {
                     Accounts = new AccountsInDTO()
                     {
                         Primary = new AccountInDTO()
                         {
                             Id = "primary",
                             Cash = 3302.52,
                         }
                     }

                }
            };

            var sessions = new ForexSessionInDTO[]{session};
            var responsePostBody = await PostAsync<ForexSessionInDTO[]>(sessions,urlpost);
            Console.WriteLine(responsePostBody);

            var trade = new ForexTradeDTO()
            {
                Pair = "VVVUSD",
                Price = 1.03,
                Units = 100,
                StopLoss = 1.11,
                TakeProfit = 1.01,
                Date = "04/30/2020 20:59:50"
            };
            var responseTradeBody =await PatchAsync<ForexTradeDTO>(trade,urlpatchtrade);
            Console.WriteLine(responseTradeBody);

            var priceTrigger = new ForexPriceDTO()
            {
                Instrument = "VVVUSD",
                Bid = 1.03,
                Ask = 1.04,
                Time = "04/30/2020 20:59:50"
            };

           var responsePriceBody = await PatchAsync<ForexPriceDTO>(priceTrigger,urlpatchprice);
            Console.WriteLine(responsePriceBody);

            var priceLow = new ForexPriceDTO()
            {
                Instrument = "VVVUSD",
                Bid = 1.09,
                Ask = 1.10,
                Time = "05/01/2020 20:59:50"
            };

            responsePriceBody = await PatchAsync<ForexPriceDTO>(priceLow,urlpatchprice);

            Console.WriteLine(responsePriceBody);

            var priceLow2 = new ForexPriceDTO()
            {
                Instrument = "VVVUSD",
                Bid = 1.08,
                Ask = 1.09,
                Time = "05/01/2020 21:59:50"
            };

            responsePriceBody = await PatchAsync<ForexPriceDTO>(priceLow2,urlpatchprice);

            Console.WriteLine(responsePriceBody);

            var priceClose = new ForexPriceDTO()
            {
                Instrument = "VVVUSD",
                Bid = 1.13,
                Ask = 1.14,
                Time = "05/02/2020 20:59:50"
            };

            responsePriceBody = await PatchAsync<ForexPriceDTO>(priceClose,urlpatchprice);

            Console.WriteLine(responsePriceBody);


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

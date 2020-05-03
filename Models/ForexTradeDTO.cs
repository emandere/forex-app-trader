using System;
using System.Text.Json.Serialization;
namespace forex_app_trader.Models
{
    public class ForexTradeDTO
    {
        [JsonPropertyName("pair")]
        public string Pair { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }
        [JsonPropertyName("stoploss")]
        public double StopLoss { get; set; }
        [JsonPropertyName("takeprofit")]
        public double TakeProfit { get; set; }
        [JsonPropertyName("price")]
        public double Price { get; set; }
        [JsonPropertyName("units")]
        public int Units { get; set; }
        [JsonPropertyName("long")]
        public bool Long { get; set; }
    }
}
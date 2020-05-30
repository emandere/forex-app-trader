using System;
using System.Text.Json.Serialization;
namespace forex_app_trader.Models
{
    public class ForexDailyPriceDTO
    {
        [JsonPropertyName("pair")]
        public string Pair { get; set; }

        [JsonPropertyName("date")]
        public string Date { get; set; }

        [JsonPropertyName("open")]
        public double Open { get; set; }

        [JsonPropertyName("high")]
        public double High { get; set; }

        [JsonPropertyName("low")]
        public double Low { get; set; }

        [JsonPropertyName("close")]
        public double Close { get; set; }

        [JsonPropertyName("datetime")]
        public DateTime Datetime { get; set; }
    }

}

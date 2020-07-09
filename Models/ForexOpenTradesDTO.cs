using System;
using System.Text.Json.Serialization;
namespace forex_app_trader.Models
{
    public  class ForexOpenTradesDTO
    {
        [JsonPropertyName("trades")]
        public ForexOpenTradeDTO [] Trades { get; set; }
    }

    public class ForexOpenTradeDTO
    {
        [JsonPropertyName("instrument")]
        public string Instrument { get; set; }

        [JsonPropertyName("initialUnits")]
        public string InitialUnits { get; set; }

        public string Pair 
        {
            get => Instrument.Replace("_","");
        }

        public long Units 
        {
            get => Convert.ToInt64(InitialUnits);
        }
    }
}

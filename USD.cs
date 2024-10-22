using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BitcoinTrackerCSharp
{
    internal class USD
    {
        [JsonPropertyName("rate_float")]
        public decimal? rate_float { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BitcoinTrackerC_
{
    internal class BPI
    {
        [JsonPropertyName("USD")]
        public USD? USD {  get; set; }
    }
}

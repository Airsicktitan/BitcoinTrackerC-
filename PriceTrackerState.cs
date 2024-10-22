using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitcoinTrackerCSharp
{
    internal sealed class PriceTrackerState
    {
        private static readonly PriceTrackerState _instance = new PriceTrackerState();
        public decimal? PrevPrice { get; set; } = null;

        private PriceTrackerState() { }

        public static PriceTrackerState Instance
        {
            get
            {
                return _instance;
            }
        }
    }
}


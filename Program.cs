using System.Text.Json;

namespace BitcoinTrackerC_
{
    internal class Program
    {
        public static decimal? prevPrice = null;
        private static readonly HttpClient client = new ();
        static async Task Main(string[] args)
        {
            int quantity = PresentUserInterface();

            while (true)
            {
                if(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Q)
                {
                    Console.WriteLine("Exiting application. Thank you!");
                    break;
                }
                await GetBitcoinPrice(quantity);
                await Task.Delay(1000);
            }
        }

        public static int PresentUserInterface()
        {
            string sep = "Enter a quantity you'd like to track: ";
            int quantity;

            while (true)
            {
                Console.Write(sep);
                string? input = Console.ReadLine();

                if (int.TryParse(input, out quantity) && quantity > 0) break;
                else Console.WriteLine("Please enter a positive value");

            }

            for (int i = 0; i < sep.Length + 2; i++) Console.Write('-');
            Console.WriteLine();

            return quantity;
        }

        public static async Task GetBitcoinPrice(int quantity = 1)
        {
            string url = "https://api.coindesk.com/v1/bpi/currentprice.json";
            decimal bitPrice;

            try
            {
                HttpResponseMessage resp = await client.GetAsync(url);

                if(!resp.IsSuccessStatusCode)
                {
                    Console.WriteLine("Error parsing data... Please try again.");
                    return;
                }
 
                string jsonData = await resp.Content.ReadAsStringAsync();
                BitcoinTracker? coin = JsonSerializer.Deserialize<BitcoinTracker>(jsonData);

                if (coin is null && coin?.BPI is null)
                {
                    Console.WriteLine("Error parsing data...");
                    return;
                }
   
                bitPrice = (decimal)coin.BPI.USD.rate_float * quantity;
                decimal? percentageChange = null;

                if (prevPrice != null) percentageChange = Math.Round(((bitPrice - prevPrice.Value) / prevPrice.Value) * 100, 5);

                if(prevPrice == null || bitPrice != prevPrice)
                {
                    Console.Write($"Current bitcoin price for {quantity} {(quantity == 1 ? "coin" : "coins")}: ");

                    if (prevPrice > bitPrice) Console.ForegroundColor = ConsoleColor.Red;
                    else if (prevPrice < bitPrice) Console.ForegroundColor = ConsoleColor.Green;

                    Console.Write($"{bitPrice:C}");

                    if (percentageChange.HasValue) Console.WriteLine($" ({percentageChange.Value:+0.00000;-0.00000}%)");
                    else Console.WriteLine();
                    Console.ResetColor();


                    prevPrice = bitPrice;
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}

using System.Text.Json;

namespace BitcoinTrackerC_
{
    internal class Program
    {
        public static decimal? prevPrice = null;
        private static readonly HttpClient client = new()
        {
            Timeout = TimeSpan.FromSeconds(10)
        };
        private static readonly int requestRate = 1_000;
        private static readonly int backOffTime = 2_000;
        private static readonly int maxRetryAttempts = 3;

        static async Task Main(string[] args)
        {
            while(true)
            {
                int quantity = PresentUserInterface();
                if (quantity == -1)
                {
                    Console.WriteLine("\nExiting application.  Thank you! \n\nPress any key to exit...");
                    Console.ReadKey();
                    break;
                }

                while (true)
                {
                    if(Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Q)
                    {
                        Console.WriteLine();
                        prevPrice = null;
                        break;
                    }
                    try
                    {
                        await GetBitcoinPriceWithRetry(quantity);
                    }
                    catch (HttpRequestException ex)
                    {
                        if (!PromptRetry($"An error has occurred: {ex.Message}. Do you want to retry? (Y/N)")) break;
                    }
                    catch (Exception ex)
                    {
                        if (!PromptRetry($"An error has occurred: {ex.Message}. Do you want to retry? (Y/N)")) break;
                    }
                    await Task.Delay(requestRate);
                }
            }
        }

        public static int PresentUserInterface()
        {
            string sep = "Enter a quantity you'd like to track (Q to exit): ";
            int quantity;

            while (true)
            {
                Console.Write(sep);
                string? input = Console.ReadLine();

                if(input?.ToUpper() == "Q") return -1;

                if (int.TryParse(input, out quantity) && quantity > 0) break;
                else Console.WriteLine("Please enter a positive value");

            }

            for (int i = 0; i < sep.Length + 2; i++) Console.Write('-');
            Console.WriteLine();

            return quantity;
        }

        public static async Task GetBitcoinPriceWithRetry(int quantity)
        {
            int retryCount = 0;
            int retryDelay = backOffTime;

            while (retryCount < maxRetryAttempts)
            {
                try
                {
                    await GetBitcoinPrice(quantity);
                    retryDelay = backOffTime;
                    return;
                }
                catch (HttpRequestException ex)
                {
                    retryCount++;
                    Console.WriteLine($"Attempt {retryCount} failed: {ex.Message}");

                    if (retryCount == maxRetryAttempts) throw;

                    Console.WriteLine($"Backing off for {retryDelay / 1000} seconds before retrying...");
                    await Task.Delay(retryDelay);
                    retryDelay *= 2;
                }
            }
        }

        public static async Task GetBitcoinPrice(int quantity = 1)
        {
            string url = "https://api.coindesk.com/v1/bpi/currentprice.json";
            decimal bitPrice;

            HttpResponseMessage resp = await client.GetAsync(url);

            if (resp.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                Console.WriteLine("Rate limit exceeded. Backing off for a longer period...");
                await Task.Delay(backOffTime * 2);
                throw new HttpRequestException("Rate limit exceeded.");
            }

            if(!resp.IsSuccessStatusCode)
            {
                Console.WriteLine($"Error parsing data... Please try again. Status Code {resp.StatusCode}");
                throw new HttpRequestException($"HTTP Request failed with status code: {resp.StatusCode}");
            }
 
            string jsonData = await resp.Content.ReadAsStringAsync();
            BitcoinTracker? coin;

            try 
            { 
                coin = JsonSerializer.Deserialize<BitcoinTracker>(jsonData); 
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error parsing JSON data: {ex.Message}");
                return;
            }

            if (coin is null || coin.BPI is null)
            {
                Console.WriteLine("Error parsing data... Coin is null or Coin BPI is null");
                return;
            }
   
            bitPrice = (decimal)coin?.BPI?.USD?.rate_float * quantity;
            decimal? percentageChange = null;

            if (prevPrice != null) percentageChange = Math.Round(((bitPrice - prevPrice.Value) / prevPrice.Value) * 100, 5);

            DisplayPriceChanges(prevPrice, percentageChange, quantity, bitPrice);
            prevPrice = bitPrice;
        }

        public static void DisplayPriceChanges(decimal? prevPrice, decimal? percentageChange, int quantity, decimal bitPrice)
        {
            if (prevPrice == null || bitPrice != prevPrice)
            {
                Console.Write($"Current bitcoin price for {quantity} {(quantity == 1 ? "coin" : "coins")}: ");

                SetConsoleColor(prevPrice, bitPrice);

                Console.Write($"{bitPrice:C}");

                if (percentageChange.HasValue) Console.WriteLine($" ({percentageChange.Value:+0.00000;-0.00000}%)");
                else Console.WriteLine();
                Console.ResetColor();
            }
        }

        public static void SetConsoleColor(decimal? prevPrice, decimal currentPrice)
        {
            if (prevPrice == null) return;

            if (prevPrice > currentPrice) Console.ForegroundColor = ConsoleColor.Red;
            else if (prevPrice < currentPrice) Console.ForegroundColor = ConsoleColor.Green;
            else Console.ResetColor();
            
        }

        public static bool PromptRetry(string message)
        {
            Console.WriteLine(message);
            return Console.ReadKey(true).Key == ConsoleKey.Y;
        }
    }
}

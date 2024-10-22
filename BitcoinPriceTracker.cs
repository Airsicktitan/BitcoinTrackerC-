using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BitcoinTrackerCSharp
{
    internal class BitcoinPriceTracker
    {
        private static readonly HttpClient client = new()
        {
            Timeout = TimeSpan.FromSeconds(10)
        };
        private static readonly int requestRate = 1_000;
        private static readonly int backOffTime = 2_000;
        private static readonly int maxRetryAttempts = 3;
        public static decimal? prevPrice = null;

        private static readonly UserInterface ui = new();
        public async Task GetBitcoinPriceWithRetry(int quantity)
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

        public async Task GetBitcoinPrice(int quantity = 1)
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

            if (!resp.IsSuccessStatusCode)
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

            if (prevPrice != null) percentageChange = Math.Round((bitPrice - prevPrice.Value) / prevPrice.Value * 100, 5);

            ui.DisplayPriceChanges(prevPrice, percentageChange, quantity, bitPrice);
            prevPrice = bitPrice;
        }
    }
}

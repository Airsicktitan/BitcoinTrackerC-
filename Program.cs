using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace BitcoinTrackerCSharp
{
    internal class Program
    {
        public static decimal? prevPrice = null;
        private static readonly int requestRate = 1_000;

        private static readonly UserInterface ui = new();
        private static readonly BitcoinPriceTracker bpt = new();

        static async Task Main(string[] args)
        {
            while (true)
            {
                int quantity = ui.PresentUserInterface();
                if (quantity == -1)
                {
                    Console.WriteLine("\nExiting application.  Thank you! \n\nPress any key to exit...");
                    Console.ReadKey();
                    break;
                }

                while (true)
                {
                    if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Q)
                    {
                        Console.WriteLine();
                        prevPrice = null;
                        PriceTrackerState.Instance.PrevPrice = prevPrice;
                        break;
                    }
                    try
                    {
                        await bpt.GetBitcoinPriceWithRetry(quantity);
                    }
                    catch (HttpRequestException ex)
                    {
                        if (!ui.PromptRetry($"An error has occurred: {ex.Message}. Do you want to retry? (Y/N)")) break;
                    }
                    catch (Exception ex)
                    {
                        if (!ui.PromptRetry($"An error has occurred: {ex.Message}. Do you want to retry? (Y/N)")) break;
                    }
                    await Task.Delay(requestRate);
                }
            }
        }
    }
}

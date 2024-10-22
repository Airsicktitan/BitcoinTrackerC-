using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitcoinTrackerCSharp
{
    internal class UserInterface
    {
        public int PresentUserInterface()
        {
            string sep = "Enter a quantity you'd like to track (Q to exit): ";
            int quantity;

            while (true)
            {
                Console.Write(sep);
                string? input = Console.ReadLine();

                if (input?.ToUpper() == "Q") return -1;

                if (int.TryParse(input, out quantity) && quantity > 0) break;
                else Console.WriteLine("Please enter a positive value");

            }

            for (int i = 0; i < sep.Length + 2; i++) Console.Write('-');
            Console.WriteLine();

            return quantity;
        }
        public bool PromptRetry(string message)
        {
            Console.WriteLine(message);
            return Console.ReadKey(true).Key == ConsoleKey.Y;
        }
        public void DisplayPriceChanges(decimal? prevPrice, decimal? percentageChange, int quantity, decimal bitPrice)
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

        private static void SetConsoleColor(decimal? prevPrice, decimal currentPrice)
        {
            if (prevPrice == null) return;

            if (prevPrice > currentPrice) Console.ForegroundColor = ConsoleColor.Red;
            else if (prevPrice < currentPrice) Console.ForegroundColor = ConsoleColor.Green;
            else Console.ResetColor();

        }
    }
}

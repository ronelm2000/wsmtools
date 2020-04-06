using System;
using System.Collections.Generic;
using System.Text;

namespace Montage.Weiss.Tools.Utilities
{
    static class ConsoleUtils
    {
        public static bool Prompted(bool isNonInteractive)
        {
            if (isNonInteractive) return false;
            var result = Console.ReadKey(false).Key == ConsoleKey.Y;
            Console.Write("\r");
            return result;
        }
    }
}

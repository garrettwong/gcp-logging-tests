using System;
using System.Linq;

namespace gcp_logging_tests.Utilities
{
    public class RandomGenerator
    {
        private Random _random = new Random();

        public string GetRandomHexNumber(int digits)
        {
            byte[] buffer = new byte[digits / 2];
            _random.NextBytes(buffer);
            string result = String.Concat(buffer.Select(x => x.ToString("X2")).ToArray());
            if (digits % 2 == 0)
                return result;
            return result + _random.Next(16).ToString("X");
        }
    }
}

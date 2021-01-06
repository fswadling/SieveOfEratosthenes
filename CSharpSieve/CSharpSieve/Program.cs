using System;
using System.Collections;
using System.Diagnostics;

namespace CSharpSieve
{
    class Program
    {
        static int ToNumList(BitArray bitArray, int[] buffer)
        {
            var len = bitArray.Length;
            int j = 0;
            for (int i = 0; i < len; i++)
            {
                var isPrime = bitArray.Get(i);
                if (isPrime)
                {
                    buffer[j] = i + 1;
                    j++;
                }
            }

            return j;
        }

        static int Sieve(int topNumber, int[] buffer)
        {
            var bitArray = new BitArray(topNumber, true);
            var sqrtNum = Math.Sqrt((double)topNumber);

            for (int i = 2; i < sqrtNum; i++)
            {
                if (bitArray[i - 1])
                {
                    for (int j = i * i; j < topNumber; j += i)
                    {
                        bitArray.Set(j - 1, false);
                    }
                }
            }

            return ToNumList(bitArray, buffer);
        }

        static int[] Sieve(int topNumber)
        {
            var buffer = new int[topNumber];
            var numberOfPrimes = Sieve(topNumber, buffer);
            var primes = buffer[0..numberOfPrimes];
            return primes;
        }

        static void Main(string[] args)
        {
            var sw = new Stopwatch();
            sw.Start();
            int target = 500000000;
            var primes = Sieve(target);
            sw.Stop();

            Console.WriteLine(sw.Elapsed.TotalMilliseconds);
        }
    }
}

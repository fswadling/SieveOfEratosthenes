using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CSharpSieve
{
    class Program
    {
        static int ToNumList(BitArray bitArray, int[] buffer, int offset = 0, int nPrimesSoFar = 0)
        {
            var len = bitArray.Length;
            int j = 0;
            for (int i = 0; i < len; i++)
            {
                var isPrime = bitArray.Get(i);
                if (isPrime)
                {
                    buffer[j + nPrimesSoFar] = i + 1 + offset;
                    j++;
                }
            }

            return j;
        }

        static int Sieve(int topNumber, int[] buffer)
        {
            var bitArray = new BitArray(topNumber, true);
            var sqrtNum = (int)Math.Sqrt(topNumber);

            for (int i = 2; i <= sqrtNum; i++)
            {
                if (bitArray[i - 1])
                {
                    for (int j = i * i; j <= topNumber; j += i)
                    {
                        bitArray.Set(j - 1, false);
                    }
                }
            }

            return ToNumList(bitArray, buffer);
        }

        static int[] Sieve(int topNumber)
        {
            if (topNumber <= 0)
                throw new Exception("No.");

            var buffer = new int[topNumber];
            var numberOfPrimes = Sieve(topNumber, buffer);
            var primes = buffer[0..numberOfPrimes];
            return primes;
        }

        static int[] ConcatNumLists(BlockingCollection<BitArray> queue, BlockingCollection<bool> readOutComplete, int[] buffer, int nPrimesSoFar, int offSet)
        {
            while (true)
            {
                try
                {
                    var bitArray = queue.Take();
                    nPrimesSoFar += ToNumList(bitArray, buffer, offSet, nPrimesSoFar);
                    readOutComplete.Add(true);
                    offSet += bitArray.Length;
                }
                catch (InvalidOperationException)
                {
                    break;
                }
            }
            return buffer[0..nPrimesSoFar];
        }

        static Task<int[]> ConcatNumListsAsync(BlockingCollection<BitArray> queue, BlockingCollection<bool> readOutComplete, int[] buffer, int nPrimesSoFar, int offSet)
        {
            return Task.Run(() => ConcatNumLists(queue, readOutComplete, buffer, nPrimesSoFar, offSet));
        }

        static int[] SegmentedSieve(int topNumber, int segmentLength)
        {
            if (segmentLength > topNumber)
                return Sieve(topNumber);

            var primes = new int[(int)topNumber];
            var nPrimes = Sieve(segmentLength, primes);

            var nSegments = topNumber / segmentLength;
            if (topNumber % segmentLength > 0)
                nSegments++;

            // As the first is already done
            nSegments--;

            var queue = new BlockingCollection<BitArray>();
            var readOutQueue = new BlockingCollection<bool>();

            var createArrayTask = ConcatNumListsAsync(queue, readOutQueue, primes, nPrimes, segmentLength);

            for (int i = 0; i < nSegments; i++)
            {
                BitArray segment = new BitArray(segmentLength, true);
                var lowerBound = (i + 1) * segmentLength + 1;
                var upperBound = lowerBound + (segmentLength - 1);
                var sqrtUpper = (int)Math.Sqrt(upperBound);

                for (int j = 1; j < primes.Length; j++)
                {
                    while (primes[j] == 0)
                    {
                        readOutQueue.Take();
                    }

                    var prime = primes[j];

                    if (prime > sqrtUpper)
                        break;

                    var n = lowerBound / prime;
                    if (n * prime < lowerBound)
                        n++;

                    var primeMultipleInRange = n * prime;
                    while (primeMultipleInRange <= upperBound)
                    {
                        var index = primeMultipleInRange - lowerBound;
                        segment.Set((int)index, false);
                        primeMultipleInRange += prime;
                    }
                }

                queue.Add(segment);
            }
            queue.CompleteAdding();
            Console.WriteLine("Done adding");
            createArrayTask.Wait();
            Console.WriteLine("Done writing");

            return createArrayTask.Result;
        }

        static void Main(string[] args)
        {
            var sw = new Stopwatch();
            sw.Start();
            int target = 500000000;
           // var primes2 = Sieve(target);
            var primes = SegmentedSieve(target, 100000);
            sw.Stop();

            Console.WriteLine(sw.Elapsed.TotalMilliseconds);
        }
    }
}

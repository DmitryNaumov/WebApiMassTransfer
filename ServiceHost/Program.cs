using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AccountService;
using Microsoft.Owin.Hosting;
using Newtonsoft.Json;

namespace ServiceHost
{
    static class Program
    {
        private const int RunCount = 3;
        private const int NumberOfItems = 100000;
        private const string BaseAddress = "http://localhost:3333";

        static void Main(string[] args)
        {
            ServicePointManager.DefaultConnectionLimit = 100;

            using (WebApp.Start<Startup>(BaseAddress))
            {
                var accountChanges = GetAccountChanges().ToArray();

                Measure("Naive", () => NaiveSolution(accountChanges));
                Measure("OneBatch", () => OneBatchSolution(accountChanges));
                Measure("MultiBatch", () => MultiBatchSolution(accountChanges));

                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        static void Measure(string name, Func<TimeSpan> benchmark)
        {
            var elapsed = Enumerable.Repeat(1, RunCount).Select(n => benchmark()).Min();

            Console.WriteLine(name);
            Console.WriteLine("======================");
            Console.WriteLine("Elapsed: " + elapsed);
            Console.WriteLine("Rate: " + NumberOfItems / elapsed.TotalMilliseconds * 1000);
            Console.WriteLine();

        }

        static TimeSpan NaiveSolution(AccountChange[] accountChanges)
        {
            var stopwatch = Stopwatch.StartNew();

            var client = new HttpClient();
            client.BaseAddress = new Uri(BaseAddress);

            foreach (var change in accountChanges)
            {
                client.SendChange(change);
            }

            return stopwatch.Elapsed;
        }

        static TimeSpan OneBatchSolution(AccountChange[] accountChanges)
        {
            var stopwatch = Stopwatch.StartNew();

            var client = new HttpClient();
            client.BaseAddress = new Uri(BaseAddress);

            client.SendChange(accountChanges);

            return stopwatch.Elapsed;
        }

        static TimeSpan MultiBatchSolution(AccountChange[] accountChanges)
        {
            var stopwatch = Stopwatch.StartNew();

            var client = new HttpClient();
            client.BaseAddress = new Uri(BaseAddress);

            var tasks = accountChanges.Buffer(1000).Select(client.SendChangeAsync).ToArray();

            Task.WaitAll(tasks);

            return stopwatch.Elapsed;
        }

        static IEnumerable<AccountChange> GetAccountChanges()
        {
            var random = new Random();

            var accountChanges = Enumerable.Range(1, NumberOfItems).Select(n => new AccountChange
            {
                AccountId = n % 10000,
                Change = -50 + 100 * random.NextDouble(),
                When = DateTime.UtcNow,
                Payload = Enumerable.Range(1, 10).Select(_ => random.NextDouble()).ToArray()
            }).ToList();

            return accountChanges;
        }

        static void SendChange(this HttpClient client, params AccountChange[] changes)
        {
            client.SendChangeAsync(changes).Wait();
        }

        static async Task SendChangeAsync(this HttpClient client, params AccountChange[] changes)
        {
            var content = new StringContent(JsonConvert.SerializeObject(changes), null, "application/json");
            var response = await client.PostAsync("api/account", content);

            if (!response.IsSuccessStatusCode)
                throw new Exception(response.ReasonPhrase);
        }

        static IEnumerable<T[]> Buffer<T>(this IEnumerable<T> items, int bufferSize)
        {
            var buffer = new List<T>();

            foreach (var item in items)
            {
                buffer.Add(item);

                if (buffer.Count == bufferSize)
                {
                    yield return buffer.ToArray();
                    buffer.Clear();
                }
            }

            yield return buffer.ToArray();
        }
    }
}

namespace URLTesting.UnitTest
{
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using URLTesting.Framework;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass()]
    public class LoadTest
    {
        private TestConfig? config;

        [TestMethod]
        public async Task RunLoadTests()
        {
            Assert.IsNotNull(config);

            var (isValid, invalidReason) = config.Validate();
            Assert.IsTrue(isValid, invalidReason);

            Func<int, string> urlMaker;

            if (!config.UseStaticImage)
            {
                var testUrls = new List<string>();
                testUrls = new List<string>(File.ReadAllLines(config.PathForURLs!));
                urlMaker = i => testUrls[i];
            }
            else
            {
                urlMaker = _ => config.StaticImageURL!;
            }

            var results = new List<RunResult>(config.SampleCount);

            for (int i = 0; i < config.SampleCount; i++)
            {
                results.Add(await BulkLoad(urlMaker, config.ThreadCount, config.BatchCount));
            }

            for (var i = 0; i < config.MinSamplesToRemove; i++)
            {
                results.RemoveAt(results.FindIndex(r => r.RPS == results.Min(r => r.RPS)));
            }

            var (avg, stdev) = GetStatistics(results.Select(r => r.RPS));
            Console.WriteLine($"Avg RPS:\t{avg:N0} ± {stdev:N0}");

            for (int i = 0; i < results.Count; i++)
            {
                var result = results[i];
                result.ShowInConsole(i == 0, "Static URL", config.UseStaticImage ? "Yes" : "No");
            }

            results.ForEach(result => result.ShowErrors());

            Assert.IsTrue(results.Exists(r => r.OK > 0), "All requests failed. Make sure the paths are correct, and the server is up!");
        }

        [TestInitialize]
        public void TestInitialize()
        {
            config = ConfigManager.TestConfig;

            GC.WaitForFullGCApproach();
            GC.GetTotalMemory(true);
            Thread.Sleep(300);
        }

        private static (double avg, double stDev) GetStatistics(IEnumerable<int> values)
        {
            var avg = values.Average();
            var sum = values.Sum(d => Math.Pow(d - avg, 2));
            var stDev = Math.Sqrt(sum / (values.Count() - 1));
            return (avg, stDev);
        }

        private static async Task<RunResult> BulkLoad(Func<int, string> urlMaker, int threadCount, int batchCount)
        {
            using HttpClient client = new() { };

            var parallelOptions = new ParallelOptions()
            {
                MaxDegreeOfParallelism = threadCount
            };

            var requests = Enumerable
                .Range(0, batchCount)
                .Select(i => new Uri(urlMaker(i)))
                .ToArray();

            long totalBytes = 0;
            int statusOK = 0, statusError = 0;

            var errors = new ConcurrentDictionary<string, string?>();

            var watch = Stopwatch.StartNew();
            await Parallel.ForEachAsync(requests, parallelOptions, async (uri, token) =>
            {
                try
                {
                    var response = await client.GetAsync(uri, token);
                    var bytes = await response.Content.ReadAsByteArrayAsync(token);

                    Interlocked.Add(ref totalBytes, bytes?.Length ?? 0);

                    if (response.IsSuccessStatusCode)
                    {
                        Interlocked.Increment(ref statusOK);
                    }
                    else
                    {
                        Interlocked.Increment(ref statusError);
                        errors["Reason"] = response.ReasonPhrase;
                    }
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref statusError);
                    errors[ex.Message] = ex.StackTrace;
                }
            });
            watch.Stop();

            double totalSeconds = watch.Elapsed.TotalSeconds;
            double totalMB = totalBytes / 1_048_576.0;
            var requestsPerSecond = batchCount / totalSeconds;
            double avgResponseSize = totalBytes / batchCount / 1024.0;
            int avgDuration = (int)watch.Elapsed.TotalMilliseconds / batchCount;
            long totalMs = (long)watch.Elapsed.TotalMilliseconds;

            return new RunResult
            {
                RPS = (int)requestsPerSecond,
                Threads = threadCount,
                BatchCount = batchCount,
                OK = statusOK,
                Error = statusError,
                TotalMB = totalMB,
                AvgKb = avgResponseSize,
                AvgMs = avgDuration,
                TotalMs = totalMs,
                Errors = errors
            };
        }
    }
}
using System.Collections.Concurrent;

namespace URLTesting.UnitTest
{
    internal class RunResult
    {
        public int RPS { get; init; }
        public int Threads { get; init; }
        public int BatchCount { get; init; }
        public int OK { get; init; }
        public int Error { get; init; }
        public double TotalMB { get; init; }
        public double AvgKb { get; init; }
        public int AvgMs { get; init; }
        public long TotalMs { get; init; }
        public ConcurrentDictionary<string, string?> Errors { get; init; } = new ConcurrentDictionary<string, string?>();

        public void ShowErrors()
        {
            foreach (var ex in Errors)
            {
                Console.WriteLine($"{ex.Key}: {ex.Value}");
            }
        }

        public void ShowInConsole(bool displayHeader, string statName, string statValue)
        {
            var show = new Dictionary<string, object>
            {
                { statName, statValue },
                { "RPS", RPS},
                { "Threads", Threads},
                { "BatchSize", BatchCount},
                { "OK", OK },
                { "Error", Error },
                { "TotalMB", $"{TotalMB:N2}" },
                { "AvgKb", $"{AvgKb:N1}" },
                { "AvgMs", AvgMs },
                { "TotalMs", TotalMs },
            };

            if (displayHeader)
            {
                Console.WriteLine(string.Join("\t", show.Keys.Select(k => $"{k,-10}")));
            }

            Console.WriteLine(string.Join("\t", show.Values.Select(k => $"{k,-10}")));
        }
    }

}
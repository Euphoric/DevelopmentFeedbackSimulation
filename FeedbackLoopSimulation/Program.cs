using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;

namespace FeedbackLoopSimulation
{
    class Program
    {
        static void Main(string[] args)
        {
            List<DevelopmentSimulation> simulations = new List<DevelopmentSimulation>()
            {
                new DevelopmentSimulation("Minimal testing"),

                new DevelopmentSimulation("Heavy manual testing")
                {
                    ManualTestSpeed = LogNormal.WithMeanVariance(16, 10 * 10),
                    ManualTestDetectionChance = new ContinuousUniform(0.5, 0.9)
                },

                new DevelopmentSimulation("Mostly integration tests")
                {
                    DailyTestSpeed = new ContinuousUniform(2, 6),
                    DailyTestDetectionChance = new ContinuousUniform(0.2, 0.6),

                    ManualTestSpeed = LogNormal.WithMeanVariance(8, 7 * 7),
                    ManualTestDetectionChance = new ContinuousUniform(0.5, 0.9)
                },

                new DevelopmentSimulation("Unit + integration tests")
                {
                    CommitTestSpeed = new ContinuousUniform(0.1, 1),
                    CommitTestDetectionChance = new ContinuousUniform(0.2, 0.6),

                    DailyTestSpeed = new ContinuousUniform(2, 6),
                    DailyTestDetectionChance = new ContinuousUniform(0.2, 0.6),

                    ManualTestSpeed = LogNormal.WithMeanVariance(4, 5 * 5),
                    ManualTestDetectionChance = new ContinuousUniform(0.5, 0.9)
                },

                new DevelopmentSimulation("Better unit + integration tests")
                {
                    CommitTestSpeed = new ContinuousUniform(0.1, 1),
                    CommitTestDetectionChance = new ContinuousUniform(0.5, 0.8),

                    DailyTestSpeed = new ContinuousUniform(2, 6),
                    DailyTestDetectionChance = new ContinuousUniform(0.5, 0.8),

                    ManualTestSpeed = LogNormal.WithMeanVariance(1, 2 * 2),
                    ManualTestDetectionChance = new ContinuousUniform(0.5, 0.9)
                },

                new DevelopmentSimulation("Continuous delivery")
                {
                    CommitTestSpeed = new ContinuousUniform(0.1, 1),
                    CommitTestDetectionChance = new ContinuousUniform(0.7, 0.9),

                    DailyTestSpeed = new ContinuousUniform(2, 6),
                    DailyTestDetectionChance = new ContinuousUniform(0.7, 0.95),

                    ManualTestSpeed = LogNormal.WithMeanVariance(0.1, 1 * 1),
                    ManualTestDetectionChance = new ContinuousUniform(0.5, 0.9)
                },

                new DevelopmentSimulation("Heavy manual testing + shorter dev time")
                {
                    DevSpeed = LogNormal.WithMeanVariance(2, 2 * 2),
                    FixSpeed = LogNormal.WithMeanVariance(0.2, 1 * 1),

                    ManualTestSpeed = LogNormal.WithMeanVariance(16, 10 * 10),
                    ManualTestDetectionChance = new ContinuousUniform(0.5, 0.9)
                },

                new DevelopmentSimulation("Continuous delivery + shorter dev time")
                {
                    DevSpeed = LogNormal.WithMeanVariance(2, 2 * 2),
                    FixSpeed = LogNormal.WithMeanVariance(0.2, 1 * 1),

                    CommitTestSpeed = new ContinuousUniform(0.1, 1),
                    CommitTestDetectionChance = new ContinuousUniform(0.7, 0.9),

                    DailyTestSpeed = new ContinuousUniform(2, 6),
                    DailyTestDetectionChance = new ContinuousUniform(0.7, 0.95),

                    ManualTestSpeed = LogNormal.WithMeanVariance(0.1, 1 * 1),
                    ManualTestDetectionChance = new ContinuousUniform(0.5, 0.9)
                }
            };

            Dictionary<string, Dictionary<int, int>> histograms = new Dictionary<string, Dictionary<int, int>>();

            foreach (var simulation in simulations)
            {
                var histogram = RunMonteCarlo(simulation);
                histograms.Add(simulation.Name, histogram);
            }

            OutputHistograms(histograms);
        }

        private static Dictionary<int, int> RunMonteCarlo(DevelopmentSimulation simulation)
        {
            List<double> successTime = new List<double>();
            List<int> repeatCounts = new List<int>();

            int missedCount = 0;
            var trialCount = 10000;

            for (int i = 0; i < trialCount; i++)
            {
                simulation.Run();

                repeatCounts.Add(simulation.FixCount);
                successTime.Add(simulation.RunTime);

                if (simulation.HasError)
                {
                    missedCount++;
                }
            }

            Console.WriteLine($"Simulation: {simulation.Name}");
            var detectionChance = missedCount / (double) trialCount * 100;
            Console.WriteLine($"Missed errors probability: {detectionChance:N1}%");
            Console.WriteLine($"Development time average: {successTime.Average():N1} h");

            var repeatCountGroups = repeatCounts.GroupBy(x => x).Select(x => new {Repeats = x.Key, Count = x.Count()})
                .OrderBy(x => x.Repeats);

            Console.WriteLine($"Fix counts:");
            foreach (var countGroup in repeatCountGroups)
            {
                Console.WriteLine($"{countGroup.Repeats}: {countGroup.Count}");
            }

            var lower = (int) Math.Floor(successTime.Min());
            var upper = (int) Math.Ceiling(successTime.OrderBy(x => x).ElementAt(successTime.Count * 995 / 1000));

            var histogram = new Histogram(successTime, upper - lower, lower, upper);

            var histogramBuckets =
                    Enumerable.Range(0, histogram.BucketCount)
                        .Select(i => histogram[i])
                        .ToDictionary(x=>(int)x.LowerBound, x=>(int)x.Count)
                ;

            //File.WriteAllLines($"histogram_{simulation.Name}.csv", histogramBuckets.Select(x => x.Key.ToString(CultureInfo.InvariantCulture)+";"+ x.Value.ToString(CultureInfo.InvariantCulture)));

            return histogramBuckets;
        }

        private static void OutputHistograms(Dictionary<string, Dictionary<int, int>> histograms)
        {
            var allBins = histograms.Values.SelectMany(x => x.Keys).ToArray();
            int min = allBins.Min();
            int max = allBins.Max();

            List<string> lines = new List<string>();

            lines.Add(string.Join(";",  new []{""}.Concat(histograms.Select(hist=>hist.Key))));

            for (int i = min; i <= max; i++)
            {
                List<string> lineValues = new List<string>();

                lineValues.Add(i.ToString());

                foreach (var histogram in histograms)
                {
                    histogram.Value.TryGetValue(i, out int count);
                    lineValues.Add(count.ToString());
                }

                lines.Add(string.Join(";", lineValues));
            }

            File.WriteAllLines($"histograms.csv", lines);
        }
    }
}

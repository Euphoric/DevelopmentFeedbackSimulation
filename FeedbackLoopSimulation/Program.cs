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
                new DevelopmentSimulation("Minimal testing")
                {
                    Description = "Development process with minimal testing. Notice high amount of errors that get through."
                },

                new DevelopmentSimulation("Heavy manual testing")
                {
                    Description = "Process where most testing is done manually. Much slower, but less errors.",

                    ManualTestSpeed = LogNormal.WithMeanVariance(16, 10 * 10),
                    ManualTestDetectionChance = new ContinuousUniform(0.5, 0.9)
                },

                new DevelopmentSimulation("Mostly integration tests")
                {
                    Description = "Testing where slow integration tests are used. Allows for lower manual testing. Faster and less errors.",

                    DailyTestSpeed = new ContinuousUniform(2, 6),
                    DailyTestDetectionChance = new ContinuousUniform(0.2, 0.6),

                    ManualTestSpeed = LogNormal.WithMeanVariance(8, 7 * 7),
                    ManualTestDetectionChance = new ContinuousUniform(0.5, 0.9)
                },

                new DevelopmentSimulation("Unit + integration tests")
                {
                    Description = "Testing with some unit tests and integration tests. Allows for low manual testing. Faster and less errors.",

                    CommitTestSpeed = new ContinuousUniform(0.1, 1),
                    CommitTestDetectionChance = new ContinuousUniform(0.2, 0.6),

                    DailyTestSpeed = new ContinuousUniform(2, 6),
                    DailyTestDetectionChance = new ContinuousUniform(0.2, 0.6),

                    ManualTestSpeed = LogNormal.WithMeanVariance(4, 5 * 5),
                    ManualTestDetectionChance = new ContinuousUniform(0.5, 0.9)
                },

                new DevelopmentSimulation("Better unit + integration tests")
                {
                    Description = "Testing with good unit tests and integration tests. Allows for minimal manual testing. Even faster and less errors.",

                    CommitTestSpeed = new ContinuousUniform(0.1, 1),
                    CommitTestDetectionChance = new ContinuousUniform(0.5, 0.8),

                    DailyTestSpeed = new ContinuousUniform(2, 6),
                    DailyTestDetectionChance = new ContinuousUniform(0.5, 0.8),

                    ManualTestSpeed = LogNormal.WithMeanVariance(1, 2 * 2),
                    ManualTestDetectionChance = new ContinuousUniform(0.5, 0.9)
                },

                new DevelopmentSimulation("Continuous delivery")
                {
                    Description = "Testing with highly reliable unit tests and integration tests and almost no manual testing. Really fast and almost no errors.",

                    CommitTestSpeed = new ContinuousUniform(0.1, 1),
                    CommitTestDetectionChance = new ContinuousUniform(0.7, 0.9),

                    DailyTestSpeed = new ContinuousUniform(2, 6),
                    DailyTestDetectionChance = new ContinuousUniform(0.7, 0.95),

                    ManualTestSpeed = LogNormal.WithMeanVariance(0.1, 1 * 1),
                    ManualTestDetectionChance = new ContinuousUniform(0.5, 0.9)
                },

                new DevelopmentSimulation("Heavy manual testing + shorter dev time")
                {
                    Description = "Same as Heavy manual testing but with shorter development times. Development speed has minimal impact when testing is slow.",

                    DevSpeed = LogNormal.WithMeanVariance(2, 2 * 2),
                    FixSpeed = LogNormal.WithMeanVariance(0.2, 1 * 1),

                    ManualTestSpeed = LogNormal.WithMeanVariance(16, 10 * 10),
                    ManualTestDetectionChance = new ContinuousUniform(0.5, 0.9)
                },

                new DevelopmentSimulation("Continuous delivery + shorter dev time")
                {
                    Description = "Same as Continuous delivery but with shorter development times. Fast development has huge impact when testing is mostly automated.",

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

            Console.WriteLine($"### {simulation.Name}");
            Console.WriteLine($"{simulation.Description}");
            var detectionChance = missedCount / (double) trialCount * 100;
            Console.WriteLine($"* Missed errors probability: {detectionChance:N1}%");
            Console.WriteLine($"* Development time average: {successTime.Average():N1} h");

            var orderedTimes = successTime.OrderBy(x => x).ToArray();

            Console.WriteLine("* 50 percentile : {0:N1} h", orderedTimes[orderedTimes.Length * 50 / 100]);
            Console.WriteLine("* 70 percentile : {0:N1} h", orderedTimes[orderedTimes.Length * 70 / 100]);
            Console.WriteLine("* 85 percentile : {0:N1} h", orderedTimes[orderedTimes.Length * 85 / 100]);
            Console.WriteLine("* 95 percentile : {0:N1} h", orderedTimes[orderedTimes.Length * 95 / 100]);

            //var repeatCountGroups = repeatCounts.GroupBy(x => x).Select(x => new { Repeats = x.Key, Count = x.Count() })
            //    .OrderBy(x => x.Repeats);

            //Console.WriteLine($"Fix counts:");
            //foreach (var countGroup in repeatCountGroups)
            //{
            //    Console.WriteLine($"{countGroup.Repeats}: {countGroup.Count}");
            //}

            var lower = (int) Math.Floor(successTime.Min());
            var upper = (int) Math.Ceiling(orderedTimes.ElementAt(successTime.Count * 995 / 1000));

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

using MathNet.Numerics.Distributions;

namespace FeedbackLoopSimulation
{
    public class DevelopmentSimulation
    {
        /// <summary>
        /// Descriptive nam'
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Chance a development change will introduce an error.
        /// </summary>
        public double ChangeErrorProbability { get; set; }
        /// <summary>
        /// Chance that fixing an error will introduce another error.
        /// </summary>
        public double FixErrorProbability { get; set; }
        /// <summary>
        /// How long development takes.
        /// </summary>
        public IContinuousDistribution DevSpeed { get; set; }
        /// <summary>
        /// How long fixing an error takes.
        /// </summary>
        public IContinuousDistribution FixSpeed { get; set; }
        /// <summary>
        /// How fast per-commit tests are.
        /// </summary>
        public IContinuousDistribution CommitTestSpeed { get; set; }
        /// <summary>
        /// Chance that per-commit tests find an error, if it exists.
        /// </summary>
        public IContinuousDistribution CommitTestDetectionChance { get; set; }
        /// <summary>
        /// How long do daily tests take after per-commits finish.
        /// </summary>
        public IContinuousDistribution DailyTestSpeed { get; set; }
        /// <summary>
        /// Chance that daily tests find and error, if it exists and per-commit tests didn't find it.
        /// </summary>
        public IContinuousDistribution DailyTestDetectionChance { get; set; }
        /// <summary>
        /// How long do manual tests take, assuming daily tests pass.
        /// </summary>
        public IContinuousDistribution ManualTestSpeed { get; set; }
        /// <summary>
        /// Chance that manual test finds an error, if it exists and daily tests didn't find it.
        /// </summary>
        public IContinuousDistribution ManualTestDetectionChance { get; set; }

        /// <summary>
        /// How long the simulation ran. Eg. whole development process.
        /// </summary>
        public double RunTime { get; private set; }

        /// <summary>
        /// How many times had the change be fixed.
        /// </summary>
        public int FixCount { get; private set; }

        /// <summary>
        /// Did the change still have an error after it went through testing.
        /// </summary>
        public bool HasError { get; private set; }

        public DevelopmentSimulation(string name)
        {
            Name = name;

            ChangeErrorProbability = 0.5;
            FixErrorProbability = 0.1;

            DevSpeed = LogNormal.WithMeanVariance(8, 10 * 10);
            FixSpeed = LogNormal.WithMeanVariance(1, 5 * 5);

            CommitTestSpeed = new ContinuousUniform(0.1, 1);
            CommitTestDetectionChance = new ContinuousUniform(0.0, 0.1);

            DailyTestSpeed = new ContinuousUniform(2, 6);
            DailyTestDetectionChance = new ContinuousUniform(0.0, 0.1);

            ManualTestSpeed = LogNormal.WithMeanVariance(4, 10 * 10);
            ManualTestDetectionChance = new ContinuousUniform(0.1, 0.3);
        }

        public void Run()
        {
            IContinuousDistribution detection = new ContinuousUniform(0, 1);

            double commitDetection = CommitTestDetectionChance.Sample();
            double dailyDetection = DailyTestDetectionChance.Sample();
            double manualDetection = ManualTestDetectionChance.Sample();

            double time = 0;

            int repeats = 0;

            bool isDev = true;

            bool hasError;
            while (true)
            {
                repeats++;
                if (isDev)
                {
                    time += DevSpeed.Sample();
                    hasError = ChangeErrorProbability > detection.Sample();

                    isDev = false;
                }
                else
                {
                    time += FixSpeed.Sample();
                    hasError = FixErrorProbability > detection.Sample();
                }

                time += CommitTestSpeed.Sample();
                if (hasError && commitDetection > detection.Sample())
                {
                    continue;
                }

                time += DailyTestSpeed.Sample();
                if (hasError && dailyDetection > detection.Sample())
                {
                    continue;
                }

                time += ManualTestSpeed.Sample();
                if (hasError && manualDetection > detection.Sample())
                {
                    continue;
                }

                break;
            }

            RunTime = time;
            FixCount = repeats - 1;
            HasError = hasError;
        }
    }
}
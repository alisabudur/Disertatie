using System.Collections.Generic;
using SoftwareQualityPrediction.Models;

namespace SoftwareQualityPrediction.Extensions
{
    public static class Extensions
    {
        public static TrainingRows ToNnModel(this IEnumerable<TrainingRow> metricsList)
        {
            var input = new List<double[]>();
            var output = new List<double[]>();

            foreach (var metrics in metricsList)
            {
                input.Add(metrics.Input);
                output.Add(metrics.Output);
            }

            return new TrainingRows
            {
                Input = input.ToArray(),
                Output = output.ToArray()
            };
        }
    }
}

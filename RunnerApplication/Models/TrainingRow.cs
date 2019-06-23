namespace SoftwareQualityPrediction.Models
{
    public class TrainingRow
    {
        public string IdColumn { get; set; }
        public double[] Input { get; set; }
        public double[] Output { get; set; }
    }
}

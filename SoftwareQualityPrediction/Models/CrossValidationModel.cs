namespace SoftwareQualityPrediction.Models
{
    public class CrossValidationModel
    {
        public TrainingModel TrainingModel { get; set; }
        public TestingModel TestingModel { get; set; }
        public int K { get; set; }
    }
}

using System.Collections.Generic;

namespace SoftwareQualityPrediction.Dtos
{
    public class TrainingDataDto
    {
        public string FilePath { get; set; }
        public string Sheet { get; set; }
        public string IdColumn { get; set; }
        public List<string> InputVariables { get; set; }
        public List<string> OutputVariables { get; set; }
    }
}

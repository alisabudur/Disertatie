using System.Collections.Generic;

namespace SoftwareQualityPrediction.Models
{
    public class TestingModel
    {
        public TestingModel() { }

        public TestingModel(TestingModel self)
        {
            NeuralNetworkPath = self.NeuralNetworkPath;
            DataFilePath = self.DataFilePath;
            Sheet = self.Sheet;
            IdColumn = self.IdColumn;
            OutputVariables = self.OutputVariables;
        }

        public string NeuralNetworkPath { get; set; }
        public string DataFilePath { get; set; }
        public string Sheet { get; set; }
        public string IdColumn { get; set; }
        public List<string> OutputVariables { get; set; }
    }
}

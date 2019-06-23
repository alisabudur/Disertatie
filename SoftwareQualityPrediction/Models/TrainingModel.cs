using System.Collections.Generic;
using System.IO;
using SoftwareQualityPrediction.Dtos;

namespace SoftwareQualityPrediction.Models
{
    public class TrainingModel
    {
        public TrainingModel() { }

        public TrainingModel(TrainingModel self)
        {
            TrainingData = self.TrainingData;
            LearningRate = self.LearningRate;
            MinError = self.MinError;
            NoOfEpochs = self.NoOfEpochs;
            HiddenLayers = self.HiddenLayers;
            ActivationFunction = self.ActivationFunction;
            NeuralNetworkName = self.NeuralNetworkName;
            NeuralNetworkSavePath = self.NeuralNetworkSavePath;
        }

        public TrainingDataModel TrainingData { get; set; }
        public double LearningRate { get; set; }
        public double MinError { get; set; }
        public int NoOfEpochs { get; set; }
        public List<int> HiddenLayers { get; set; }
        public ActivationFunction ActivationFunction { get; set; }
        public string NeuralNetworkName { get; set; }
        public string NeuralNetworkSavePath { get; set; }
        public string NeuralNetworkPath => Path.Combine(NeuralNetworkSavePath, $"{NeuralNetworkName}.txt").Replace(@"\\", @"\");
    }
}

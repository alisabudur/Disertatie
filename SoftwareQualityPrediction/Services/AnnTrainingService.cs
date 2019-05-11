using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Accord.Neuro;
using Accord.Neuro.Learning;
using SoftwareQualityPrediction.Extensions;
using SoftwareQualityPrediction.Models;

namespace SoftwareQualityPrediction.Services
{
    public class AnnTrainingService
    {
        public AnnTrainingService(double minError,
            double learningRate,
            int noEpochs,
            int noOfInputNeurons,
            int noOfOutputNeurons,
            List<int> hiddenLayers,
            ActivationFunction activationFunction,
            IEnumerable<TrainingRow> trainingRows,
            ProgressChangedEventHandler progressChangedEventHandler,
            string savePath)
        {
            _minError = minError;
            _learningRate = learningRate;
            _noEpochs = noEpochs;
            _progressChangedEventHandler = progressChangedEventHandler;
            _savePath = savePath;
            _hiddenLayers = hiddenLayers;
            _activationFunction = activationFunction;
            _trainingRows = trainingRows;
            _noOfInputNeurons = noOfInputNeurons;
            _noOfOutputNeurons = noOfOutputNeurons;
            _worker = new BackgroundWorker();
        }

        public void StartTraining()
        {
            _worker.DoWork += worker_DoWork;
            _worker.ProgressChanged += _progressChangedEventHandler;
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;
            _worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            var annInfo = new AnnInfo
            {
                LearningRate = _learningRate,
                Epochs = _noEpochs,
                Error = _minError
            };
            _worker.RunWorkerAsync(annInfo);
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender;
            var parameter = (AnnInfo)e.Argument;
            var annLearningRate = parameter.LearningRate;
            var annEpochs = parameter.Epochs;
            var annError = parameter.Error;

            _hiddenLayers.Add(_noOfOutputNeurons);

            var network = new ActivationNetwork(_activationFunctions[_activationFunction],
                _noOfInputNeurons,
                _hiddenLayers.ToArray());

            var learning = new BackPropagationLearning(network)
            {
                LearningRate = annLearningRate,
            };

            var data = _trainingRows.ToNnModel();

            var needToStop = false;
            var epoch = 0;

            while (!needToStop && epoch < annEpochs)
            {
                var error = learning.RunEpoch(data.Input, data.Output) / data.Input.Length;

                worker.ReportProgress((epoch * 100) / annEpochs);
                if (error < annError)
                    needToStop = true;
                epoch++;
            }

            worker.ReportProgress(100);
            e.Result = network;
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var worker = (BackgroundWorker)sender;
            worker.DoWork -= worker_DoWork;
            worker.ProgressChanged -= _progressChangedEventHandler;
            worker.RunWorkerCompleted -= worker_RunWorkerCompleted;
            var network = (Network)e.Result;

            if (File.Exists(_savePath))
                File.Delete(_savePath);

            network.Save(_savePath);
        }

        private class AnnInfo
        {
            public double LearningRate { get; set; }
            public int Epochs { get; set; }
            public double Error { get; set; }
        }

        private IDictionary<ActivationFunction, IActivationFunction> _activationFunctions
            = new Dictionary<ActivationFunction, IActivationFunction>
            {
                {ActivationFunction.Sigmoid, new SigmoidFunction()},
                {ActivationFunction.BipolarSigmoid, new BipolarSigmoidFunction()}
            };

        private BackgroundWorker _worker;
        private ProgressChangedEventHandler _progressChangedEventHandler;
        private IEnumerable<TrainingRow> _trainingRows;
        private double _minError;
        private double _learningRate;
        private int _noEpochs;
        private int _noOfInputNeurons;
        private int _noOfOutputNeurons;
        private List<int> _hiddenLayers;
        private ActivationFunction _activationFunction;
        private string _savePath;
    }
}

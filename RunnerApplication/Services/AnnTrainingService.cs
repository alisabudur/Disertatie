using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Accord.Neuro;
using Accord.Neuro.Learning;
using DataAccess.Repositories;
using SoftwareQualityPrediction.Extensions;
using SoftwareQualityPrediction.Models;

namespace SoftwareQualityPrediction.Services
{
    public class AnnTrainingService
    {
        public AnnTrainingService(double minError,
            double learningRate,
            int noEpochs,
            string savePath,
            string filePath,
            string sheet,
            string idColumn,
            List<string> inputNeurons,
            List<string> outputNeurons,
            List<int> hiddenLayers,
            ActivationFunction activationFunction,
            ProgressChangedEventHandler progressChangedEventHandler)
        {
            _minError = minError;
            _learningRate = learningRate;
            _noEpochs = noEpochs;
            _progressChangedEventHandler = progressChangedEventHandler;
            _savePath = savePath;
            _hiddenLayers = hiddenLayers;
            _activationFunction = activationFunction;
            _inputNeurons = inputNeurons;
            _outputNeurons = outputNeurons;
            _worker = new BackgroundWorker();

            _excelService = new ExcelService(filePath,
                sheet,
                idColumn,
                _inputNeurons,
                _outputNeurons);

            var fileName = "NeuralNetworks.xls";
            var nnFilePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            if (!File.Exists(nnFilePath))
                File.Create(nnFilePath);

            var connectionString = $"Provider=Microsoft.Jet.OLEDB.4.0; data source={nnFilePath}; Extended Properties=Excel 8.0;";
            _nnRepository = new Repository<NnModel>(connectionString);
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

            _hiddenLayers.Add(_outputNeurons.Count);

            var network = new ActivationNetwork(_activationFunctions[_activationFunction],
                _inputNeurons.Count,
                _hiddenLayers.ToArray());

            var learning = new BackPropagationLearning(network)
            {
                LearningRate = annLearningRate,
            };

            var trainingRows = _excelService.GetAllRows();
            var data = trainingRows.ToNnModel();

            var needToStop = false;
            var epoch = 1;

            while (!needToStop && epoch <= annEpochs)
            {
                var error = learning.RunEpoch(data.Input, data.Output) / data.Input.Length;

                worker.ReportProgress((epoch * 100) / annEpochs);
                if (error < annError)
                {
                    needToStop = true;
                    worker.ReportProgress(100);
                }

                epoch++;
            }
            
            e.Result = network;
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var worker = (BackgroundWorker)sender;
            worker.DoWork -= worker_DoWork;
            worker.ProgressChanged -= _progressChangedEventHandler;
            worker.RunWorkerCompleted -= worker_RunWorkerCompleted;
            var network = (Network)e.Result;

            var nnModel = new NnModel
            {
                Path = _savePath.Replace(@"\\", @"\"),
                InputNodes = string.Join(";", _inputNeurons),
                OutputNodes = string.Join(";", _outputNeurons)
            };

            if (File.Exists(_savePath))
            {
                File.Delete(_savePath);
                _nnRepository.Update(nnModel);
            }
            else
            {
                _nnRepository.Add(nnModel);
            }

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
        private double _minError;
        private double _learningRate;
        private int _noEpochs;
        private List<string> _inputNeurons;
        private List<string> _outputNeurons;
        private List<int> _hiddenLayers;
        private ActivationFunction _activationFunction;
        private string _savePath;
        private IRepository<NnModel> _nnRepository;
        private ExcelService _excelService;
    }
}

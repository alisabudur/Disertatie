using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Accord.Neuro;
using Accord.Neuro.Learning;
using DataAccess.Repositories;
using SoftwareQualityPrediction.Extensions;
using SoftwareQualityPrediction.Models;

namespace SoftwareQualityPrediction.Services
{
    public class AnnTrainingService : IService
    {
        public AnnTrainingService(TrainingModel trainingModel,
            ProgressChangedEventHandler progressChangedEventHandler)
        {
            _trainingModel = trainingModel;
            

            var fileName = "NeuralNetworks.xls";
            var nnFilePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            if (!File.Exists(nnFilePath))
                File.Create(nnFilePath);

            var connectionString = $"Provider=Microsoft.Jet.OLEDB.4.0; data source={nnFilePath}; Extended Properties=Excel 8.0;";
            _nnRepository = new Repository<NnModel>(connectionString);
        }

        public IService Succesor { get; set; }

        public Action OnCompleteCallback { get; set; }

        public void Start(int? noOfSubsets = null, int? testingSubsetIndex = null)
        {
            _excelService = new ExcelService(_trainingModel.TrainingData.FilePath,
                _trainingModel.TrainingData.Sheet,
                _trainingModel.TrainingData.IdColumn,
                _trainingModel.TrainingData.InputVariables,
                _trainingModel.TrainingData.OutputVariables);

            _worker = new BackgroundWorker();
            _worker.DoWork += worker_DoWork;

            if(_progressChangedEventHandler != null)
                _worker.ProgressChanged += _progressChangedEventHandler;

            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;
            _worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            var workerInfo = new WorkerInfo
            {
                LearningRate = _trainingModel.LearningRate,
                Epochs = _trainingModel.NoOfEpochs,
                Error = _trainingModel.MinError,
                TrainingRows = GetTrainingRows(noOfSubsets, testingSubsetIndex),
                NoOfSubsets = noOfSubsets,
                TestingSubsetIndex = testingSubsetIndex
            };
            _worker.RunWorkerAsync(workerInfo);
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender;
            var parameter = (WorkerInfo)e.Argument;
            var annLearningRate = parameter.LearningRate;
            var annEpochs = parameter.Epochs;
            var annError = parameter.Error;
            var trainingRows = parameter.TrainingRows;

            var hiddenLayersAndOutputLayer = _trainingModel.HiddenLayers;
            hiddenLayersAndOutputLayer.Add(_trainingModel.TrainingData.OutputVariables.Count);

            var inputLayer = _trainingModel.TrainingData.InputVariables.Count;

            var network = new ActivationNetwork(_activationFunctions[_trainingModel.ActivationFunction],
                inputLayer,
                hiddenLayersAndOutputLayer.ToArray());

            var learning = new BackPropagationLearning(network)
            {
                LearningRate = annLearningRate
            };

            var data = trainingRows.ToNnModel();

            var needToStop = false;
            var epoch = 0;

            while (!needToStop && epoch < annEpochs)
            {
                var error = learning.RunEpoch(data.Input, data.Output) 
                            / data.Input.Length;

                worker.ReportProgress((epoch * 100) / annEpochs);
                if (error < annError)
                    needToStop = true;
                epoch++;
            }

            e.Result = new WorkerResultInfo
            {
                Network = network,
                NoOfSubsets = parameter.NoOfSubsets,
                TestingSubsetIndex = parameter.TestingSubsetIndex
            };
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var worker = (BackgroundWorker)sender;
            worker.DoWork -= worker_DoWork;
            worker.ProgressChanged -= _progressChangedEventHandler;
            worker.RunWorkerCompleted -= worker_RunWorkerCompleted;
            var parameter = (WorkerResultInfo)e.Result;
            var network = parameter.Network;

            var savePath = _trainingModel.NeuralNetworkPath;

            var nnModel = new NnModel
            {
                Path = savePath,
                InputNodes = string.Join(";", _trainingModel.TrainingData.InputVariables),
                OutputNodes = string.Join(";", _trainingModel.TrainingData.OutputVariables)
            };

            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                _nnRepository.Update(nnModel);
            }
            else
            {
                _nnRepository.Add(nnModel);
            }

            network.Save(savePath);

            if (parameter.TestingSubsetIndex.HasValue && parameter.NoOfSubsets.HasValue)
            {
                OnCompleteCallback?.Invoke();
                Succesor?.Start(parameter.NoOfSubsets, parameter.TestingSubsetIndex);
            }
        }

        private IEnumerable<TrainingRow> GetTrainingRows(int? noOfSubsets = null, int? testingSubsetIndex = null)
        {
            List<TrainingRow> testingRows = null;

            if (!noOfSubsets.HasValue || !testingSubsetIndex.HasValue)
            {
                testingRows = _excelService.GetAllRows().ToList();
            }
            else
            {
                testingRows = new List<TrainingRow>();
                var totalRowsCount = _excelService.GetAllRows().Count();
                var subsetCount = (int)Math.Ceiling((double)totalRowsCount / noOfSubsets.Value);

                for (var i = 0; i < noOfSubsets; i++)
                {
                    if(i == testingSubsetIndex)
                        continue;

                    var rows = _excelService.GetAllRows()
                        .Skip(subsetCount * i)
                        .Take(subsetCount)
                        .ToList();

                    testingRows.AddRange(rows);
                }
            }

            return testingRows;
        }

        private class WorkerInfo
        {
            public double LearningRate { get; set; }
            public int Epochs { get; set; }
            public double Error { get; set; }
            public IEnumerable<TrainingRow> TrainingRows { get; set; }
            public int? NoOfSubsets { get; set; }
            public int? TestingSubsetIndex { get; set; }
        }

        private class WorkerResultInfo
        {
            public int? NoOfSubsets { get; set; }
            public int? TestingSubsetIndex { get; set; }
            public Network Network { get; set; }
        }

        private IDictionary<ActivationFunction, IActivationFunction> _activationFunctions
            = new Dictionary<ActivationFunction, IActivationFunction>
            {
                {ActivationFunction.Sigmoid, new SigmoidFunction(1)},
                {ActivationFunction.BipolarSigmoid, new BipolarSigmoidFunction(1)}
            };

        private BackgroundWorker _worker;
        private ProgressChangedEventHandler _progressChangedEventHandler;
        private TrainingModel _trainingModel;
        private IRepository<NnModel> _nnRepository;
        private ExcelService _excelService;
    }
}

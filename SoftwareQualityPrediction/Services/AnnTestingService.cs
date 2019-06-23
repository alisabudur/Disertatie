using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Accord.Neuro;
using DataAccess.Repositories;
using SoftwareQualityPrediction.Models;

namespace SoftwareQualityPrediction.Services
{
    public class AnnTestingService : IService
    {
        public AnnTestingService(TestingModel testingModel,
            ProgressChangedEventHandler progressChangedEventHandler)
        {
            _testingModel = testingModel;
            _progressChangedEventHandler = progressChangedEventHandler;
        }

        public IService Succesor { get; set; }

        public Action OnCompleteCallback { get; set; }

        public static List<string> GetNeuralNetworkInputVariables(string neuralNetworkPath)
        {
            var fileName = "NeuralNetworks.xls";
            var nnFilePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            var connectionString = $"Provider=Microsoft.Jet.OLEDB.4.0; data source={nnFilePath}; Extended Properties=Excel 8.0;";
            var nnRepository = new Repository<NnModel>(connectionString);
            var model = nnRepository.GetSingle(neuralNetworkPath);

            return model.InputNodes.Split(';').ToList();
        }

        public void Start(int? noOfSubsets = null, int? testingSubsetIndex = null)
        {
            var inputVariables = GetNeuralNetworkInputVariables(_testingModel.NeuralNetworkPath);

            _excelService = new ExcelService(_testingModel.DataFilePath,
                _testingModel.Sheet,
                _testingModel.IdColumn,
                inputVariables,
                _testingModel.OutputVariables);

            _worker = new BackgroundWorker();
            _worker.DoWork += worker_DoWork;

            if (_progressChangedEventHandler != null)
                _worker.ProgressChanged += _progressChangedEventHandler;

            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;
            _worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            var workerInfo = new WorkerInfo
            {
                NoOfSubsets = noOfSubsets,
                TestingSubsetIndex = testingSubsetIndex,
                TestingRows = GetTestingRows(noOfSubsets, testingSubsetIndex)
            };
            _worker.RunWorkerAsync(workerInfo);
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender;
            var parameter = (WorkerInfo)e.Argument;
            var data = parameter.TestingRows;

            _network = Network.Load(_testingModel.NeuralNetworkPath);

            var networkResult = data.ToList();
            var dataCount = data.Count();
            var index = 0;

            foreach (var inputItem in networkResult)
            {
                var input = inputItem.Input;
                var output = _network.Compute(input);
                inputItem.Output = output;

                _excelService.UpdateRow(inputItem);
                index++;
                worker.ReportProgress((index*100)/dataCount);
            }

            e.Result = new WorkerResultInfo
            {
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

            if (parameter.TestingSubsetIndex.HasValue && parameter.NoOfSubsets.HasValue)
            {
                OnCompleteCallback?.Invoke();
                Succesor?.Start();
            }
        }

        private IEnumerable<TrainingRow> GetTestingRows(int? noOfSubsets = null, int? testingSubsetIndex = null)
        {
            IEnumerable<TrainingRow> trainingRows = null;

            if (!noOfSubsets.HasValue || !testingSubsetIndex.HasValue)
            {
                trainingRows = _excelService.GetAllRows();
            }
            else
            {
                var totalRowsCount = _excelService.GetAllRows().Count();
                var subsetCount = (int)Math.Ceiling((double)totalRowsCount / noOfSubsets.Value);

                trainingRows = _excelService.GetAllRows()
                    .Skip(subsetCount * testingSubsetIndex.Value)
                    .Take(subsetCount)
                    .ToList();
            }

            return trainingRows;
        }

        private class WorkerInfo
        {
            public int? NoOfSubsets { get; set; }
            public int? TestingSubsetIndex { get; set; }
            public IEnumerable<TrainingRow> TestingRows { get; set; }
        }

        private class WorkerResultInfo
        {
            public int? NoOfSubsets { get; set; }
            public int? TestingSubsetIndex { get; set; }
        }

        private Network _network;
        private TestingModel _testingModel;
        private ExcelService _excelService;
        private BackgroundWorker _worker;
        private ProgressChangedEventHandler _progressChangedEventHandler;
    }
}

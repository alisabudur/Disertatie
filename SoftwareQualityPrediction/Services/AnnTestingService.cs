using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Accord.Neuro;
using DataAccess.Repositories;
using SoftwareQualityPrediction.Models;

namespace SoftwareQualityPrediction.Services
{
    public class AnnTestingService
    {
        public AnnTestingService(string neuralNetworkPath,
            string filePath,
            string sheet,
            string idColumn,
            List<string> outputVariables,
            ProgressChangedEventHandler progressChangedEventHandler)
        {
            _neuralNetworkPath = neuralNetworkPath;

            var inputVariables = GetNeuralNetworkModel(neuralNetworkPath).InputNodes.Split(';').ToList();

            _excelService = new ExcelService(filePath,
                sheet,
                idColumn,
                inputVariables,
                outputVariables);

            _progressChangedEventHandler = progressChangedEventHandler;
            _worker = new BackgroundWorker();
        }

        public static NnModel GetNeuralNetworkModel(string neuralNetworkPath)
        {
            var fileName = "NeuralNetworks.xls";
            var nnFilePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
            var connectionString = $"Provider=Microsoft.Jet.OLEDB.4.0; data source={nnFilePath}; Extended Properties=Excel 8.0;";
            var nnRepository = new Repository<NnModel>(connectionString);
            var model = nnRepository.GetSingle(neuralNetworkPath);

            return model;
        }

        public void StartTesting()
        {
            _worker.DoWork += worker_DoWork;
            _worker.ProgressChanged += _progressChangedEventHandler;
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;
            _worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            _worker.RunWorkerAsync();
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender;
            var data = _excelService.GetAllRows();
            _network = Network.Load(_neuralNetworkPath);

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
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var worker = (BackgroundWorker)sender;
            worker.DoWork -= worker_DoWork;
            worker.ProgressChanged -= _progressChangedEventHandler;
            worker.RunWorkerCompleted -= worker_RunWorkerCompleted;
        }

        private Network _network;
        private string _neuralNetworkPath;
        private ExcelService _excelService;
        private BackgroundWorker _worker;
        private ProgressChangedEventHandler _progressChangedEventHandler;
    }
}

using System.Collections.Generic;
using System.ComponentModel;
using SoftwareQualityPrediction.Models;

namespace SoftwareQualityPrediction.Services
{
    public class CrossValidationService
    {
        public CrossValidationService(CrossValidationModel crossValidationModel,
            ProgressChangedEventHandler crossValidationProgressChanged)
        {
            _worker = new BackgroundWorker();
            _k = crossValidationModel.K;
            _trainingModel = crossValidationModel.TrainingModel;
            _testingModel = crossValidationModel.TestingModel;
            _progressChangedEventHandler = crossValidationProgressChanged;
        }

        public void Start()
        {
            _worker.DoWork += worker_DoWork;
            _worker.ProgressChanged += _progressChangedEventHandler;
            _worker.WorkerReportsProgress = true;
            _worker.WorkerSupportsCancellation = true;
            _worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            var workerInfo = new WorkerInfo
            {
                K = _k,
                TrainingModel = _trainingModel,
                TestingModel = _testingModel
            };

            _worker.RunWorkerAsync(workerInfo);
        }

        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender;
            var parameter = (WorkerInfo)e.Argument;
            var k = parameter.K;
            var trainingModel = parameter.TrainingModel;
            var testingModel = parameter.TestingModel;

            var initialName = trainingModel.NeuralNetworkName;
            var trainingServices = new AnnTrainingService[k];
            var testingServices = new AnnTestingService[k];

            for (var i = 0; i < k; i++)
            {
                trainingModel.NeuralNetworkName = $"{initialName}-cross-validation-{i + 1}";
                testingModel.NeuralNetworkPath = trainingModel.NeuralNetworkPath;

                var trainingService = new AnnTrainingService(new TrainingModel(trainingModel), null);
                trainingService.OnCompleteCallback = MarkTrainingServiceAsCompleted;

                var testingService = new AnnTestingService(new TestingModel(testingModel), null);
                testingService.OnCompleteCallback = MarkTestingServiceAsCompleted;
                testingServices[i] = testingService;

                trainingService.Succesor = testingServices[i];

                trainingServices[i] = trainingService;

                trainingServices[i].Start(k, i);
            }
        }

        private void MarkTrainingServiceAsCompleted()
        {
            _progressValue++;
            _progressChangedEventHandler.Invoke(this, new ProgressChangedEventArgs((_progressValue * 50)/_k, null));
        }

        private void MarkTestingServiceAsCompleted()
        {
            _progressValue++;
            _progressChangedEventHandler.Invoke(this, new ProgressChangedEventArgs((_progressValue * 50) / _k, null));
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var worker = (BackgroundWorker)sender;
            worker.DoWork -= worker_DoWork;
            worker.ProgressChanged -= _progressChangedEventHandler;
            worker.RunWorkerCompleted -= worker_RunWorkerCompleted;
        }

        private class WorkerInfo
        {
            public int K { get; set; }
            public TrainingModel TrainingModel { get; set; }
            public TestingModel TestingModel { get; set; }
        }

        private int _progressValue;
        private int _k;
        private BackgroundWorker _worker;
        private TrainingModel _trainingModel;
        private TestingModel _testingModel;
        private ProgressChangedEventHandler _progressChangedEventHandler;
    }
}

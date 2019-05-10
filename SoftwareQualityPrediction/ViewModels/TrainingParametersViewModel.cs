using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using SoftwareQualityPrediction.Dtos;
using SoftwareQualityPrediction.Services;
using SoftwareQualityPrediction.Utils;

namespace SoftwareQualityPrediction.ViewModels
{
    public class TrainingParametersViewModel: BaseViewModel, IDataErrorInfo
    {
        public TrainingParametersViewModel()
        {
            _canExecute = true;
            _progressBarValue = 0;
        }

        public ICommand StartTrainingCommand => _startTraining ?? (_startTraining = new CommandHandler(StartTraining, _canExecute));

        public double MinError
        {
            get { return _minError; }
            set
            {
                _minError = value;
                OnPropertyChanged(nameof(MinError));
            }
        }

        public double LearningRate
        {
            get { return _learningRate; }
            set
            {
                _learningRate = value;
                OnPropertyChanged(nameof(LearningRate));
            }
        }

        public int NoEpochs
        {
            get { return _noEpochs; }
            set
            {
                _noEpochs = value;
                OnPropertyChanged(nameof(NoEpochs));
            }
        }

        public int CrossValidationK
        {
            get { return _crossValidationK; }
            set
            {
                _crossValidationK = value;
                OnPropertyChanged(nameof(CrossValidationK));
            }
        }

        public int ProgressBarValue
        {
            get { return _progressBarValue; }
            set
            {
                _progressBarValue = value;
                OnPropertyChanged(nameof(ProgressBarValue));
            }
        }

        public void Populate(TrainingDataDto trainingDataDto)
        {
            _trainingDataDto = trainingDataDto;
        }

        #region IDataErrorInfo

        public string Error
        {
            get { return null; }
        }

        public string this[string columnName]
        {
            get
            {
                string error = null;

                switch (columnName)
                {
                    case nameof(LearningRate):
                        if (_learningRate < 0)
                            error = Properties.Resources.LearningRateValidationMessage;
                        break;

                    case nameof(NoEpochs):
                        if (_noEpochs < 0)
                            error = Properties.Resources.NoEpochsValidationMessage;
                        break;

                    case nameof(MinError):
                        if (_minError < 0)
                            error = Properties.Resources.MinErrorValidationMessage;
                        break;
                }
                return (error);
            }
        }
        #endregion

        private void StartTraining()
        {
            var excelService = new ExcelService(_trainingDataDto.FilePath,
                _trainingDataDto.Sheet,
                _trainingDataDto.IdColumn,
                _trainingDataDto.InputVariables,
                _trainingDataDto.OutputVariables);

            var data = excelService.GetAllRows();

            var annSevice = new AnnTrainingService(_minError,
                _learningRate,
                _noEpochs,
                TrainingProgressChanged,
                data, 
                _trainingDataDto.InputVariables.Count,
                _trainingDataDto.OutputVariables.Count);

            annSevice.StartTraining();
        }

        private void TrainingProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBarValue = e.ProgressPercentage;
        }

        private bool _canExecute;
        private double _minError;
        private double _learningRate;
        private int _noEpochs;
        private int _crossValidationK;
        private int _progressBarValue;
        private TrainingDataDto _trainingDataDto;
        private ICommand _startTraining;
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Windows.Input;
using SoftwareQualityPrediction.Dtos;
using SoftwareQualityPrediction.Models;
using SoftwareQualityPrediction.Services;
using SoftwareQualityPrediction.Utils;

namespace SoftwareQualityPrediction.ViewModels
{
    public class TrainingParametersViewModel : BaseViewModel, IDataErrorInfo
    {
        public TrainingParametersViewModel()
        {
            _canExecute = true;
            _progressBarValue = 0;
            _minError = "0";
            _learningRate = "0";
            _noEpochs = "1";
            _selectedActivationFunction = ActivationFunction.Sigmoid;
            _errorList = new Dictionary<string, string>();
        }

        public ICommand StartTrainingCommand => _startTraining ?? (_startTraining = new CommandHandler(StartTraining, _canExecute));
        public ICommand SelectNeuralNetworkSavePathCommand
            => _selectNeuralNetworkSavePath ?? (_selectNeuralNetworkSavePath = new CommandHandler(SelectNeuralNetworkSavePath, _canExecute));

        public string MinError
        {
            get { return _minError; }
            set
            {
                _minError = value;
                OnPropertyChanged(nameof(MinError));
            }
        }

        public string LearningRate
        {
            get { return _learningRate; }
            set
            {
                _learningRate = value;
                OnPropertyChanged(nameof(LearningRate));
            }
        }

        public string NoEpochs
        {
            get { return _noEpochs; }
            set
            {
                _noEpochs = value;
                OnPropertyChanged(nameof(NoEpochs));
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

        public string NeuralNetworkName
        {
            get { return _neuralNetworkName; }
            set
            {
                _neuralNetworkName = value;
                OnPropertyChanged(nameof(NeuralNetworkName));
            }
        }

        public string NeuralNetworkSavePath
        {
            get { return _neuralNetworkSavePath; }
            set
            {
                _neuralNetworkSavePath = value;
                OnPropertyChanged(nameof(NeuralNetworkSavePath));
            }
        }

        public ActivationFunction SelectedActivationFunction
        {
            get { return _selectedActivationFunction; }
            set
            {
                _selectedActivationFunction = value;
                OnPropertyChanged(nameof(SelectedActivationFunction));
            }
        }

        public void Populate(TrainingDataDto trainingDataDto)
        {
            _trainingDataDto = trainingDataDto;
        }

        #region IDataErrorInfo

        public string Error
        {
            get
            {
                if (_errorList.Count == 0)
                    return string.Empty;

                return _errorList.FirstOrDefault().Value;
            }
        }

        public string this[string columnName]
        {
            get
            {
                string error = null;
                var doubleRegex = new Regex("^[0-9]+(.[0-9]+)?$");
                var intRegex = new Regex("^[1-9][0-9]*$");

                if (columnName == nameof(LearningRate))
                {
                    if (string.IsNullOrEmpty(_learningRate))
                    {
                        error = string.Format(Properties.Resources.FieldIsRequiredValidationMessage,
                            Properties.Resources.LearningRateCaption);
                    }
                    if (!doubleRegex.IsMatch(_learningRate))
                    {
                        error = Properties.Resources.LearningRateValidationMessage;
                    }
                }
                if (columnName == nameof(NoEpochs))
                {
                    if (string.IsNullOrEmpty(_noEpochs))
                    {
                        error = string.Format(Properties.Resources.FieldIsRequiredValidationMessage,
                            Properties.Resources.NumberOfEpochsCaption);
                    }

                    if (!intRegex.IsMatch(_noEpochs))
                    {
                        error = Properties.Resources.NoEpochsValidationMessage;
                    }
                }
                if (columnName == nameof(MinError))
                {
                    if (string.IsNullOrEmpty(_minError))
                    {
                        error = string.Format(Properties.Resources.FieldIsRequiredValidationMessage,
                            Properties.Resources.MinimumErrorCaption);
                    }

                    if (!doubleRegex.IsMatch(_minError))
                    {
                        error = Properties.Resources.MinErrorValidationMessage;
                    }
                }
                if (columnName == nameof(NeuralNetworkName))
                {
                    if (string.IsNullOrEmpty(_neuralNetworkName))
                    {
                        error = string.Format(Properties.Resources.FieldIsRequiredValidationMessage,
                            Properties.Resources.NeuralNetworkNameCaption);
                    }
                }
                if (columnName == nameof(NeuralNetworkSavePath))
                {
                    if (string.IsNullOrEmpty(_neuralNetworkSavePath))
                    {
                        error = string.Format(Properties.Resources.FieldIsRequiredValidationMessage,
                            Properties.Resources.LocationToSaveCaption);
                    }

                    if (!Directory.Exists(_neuralNetworkSavePath))
                    {
                        error = Properties.Resources.LocationPathNoExistValidationMessage;
                    }
                }

                if (error != null && !_errorList.ContainsKey(columnName))
                    _errorList.Add(columnName, error);

                if (error == null && _errorList.ContainsKey(columnName))
                    _errorList.Remove(columnName);

                OnPropertyChanged(nameof(Error));

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

            var annSevice = new AnnTrainingService(Convert.ToDouble(_minError),
                Convert.ToDouble(_learningRate),
                Convert.ToInt32(_noEpochs),
                _trainingDataDto.InputVariables.Count,
                _trainingDataDto.OutputVariables.Count,
                data,
                TrainingProgressChanged,
                Path.Combine(NeuralNetworkSavePath, $"{NeuralNetworkName}.txt"));

            annSevice.StartTraining();
        }

        private void SelectNeuralNetworkSavePath()
        {
            using (var op = new FolderBrowserDialog())
            {
                DialogResult result = op.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(op.SelectedPath))
                {
                    NeuralNetworkSavePath = op.SelectedPath;
                }
            }
        }

        private void TrainingProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBarValue = e.ProgressPercentage;
        }

        private bool _canExecute;
        private string _minError;
        private string _learningRate;
        private string _noEpochs;
        private int _progressBarValue;
        private string _neuralNetworkName;
        private string _neuralNetworkSavePath;
        private IDictionary<string, string> _errorList;
        private ActivationFunction _selectedActivationFunction;
        private TrainingDataDto _trainingDataDto;
        private ICommand _startTraining;
        private ICommand _selectNeuralNetworkSavePath;
    }
}

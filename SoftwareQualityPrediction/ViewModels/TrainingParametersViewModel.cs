using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
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
            _progressBarValue = 0;
            _minError = "0";
            _learningRate = "0";
            _noEpochs = "1";
            _selectedActivationFunction = ActivationFunction.Sigmoid;
            _errorList = new Dictionary<string, string>();
            _hiddenLayers = "1";
            _neuralNetworkName = "default-name";
            _neuralNetworkSavePath = @"C:\Users\alisa\\Desktop";
            _trainingCompletedMessageVisibility = Visibility.Hidden;
        }

        public ICommand StartTrainingCommand
            => _startTraining ?? (_startTraining = new CommandHandler(StartTraining, () => StartTrainingCanExecute));

        public ICommand SelectNeuralNetworkSavePathCommand
            => _selectNeuralNetworkSavePath ?? (_selectNeuralNetworkSavePath = new CommandHandler(SelectNeuralNetworkSavePath,
                   () => true));

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

        public string HiddenLayers
        {
            get { return _hiddenLayers; }
            set
            {
                _hiddenLayers = value;
                OnPropertyChanged(nameof(HiddenLayers));
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

        public Visibility TrainingCompletedMessageVisibility
        {
            get { return _trainingCompletedMessageVisibility; }
            set
            {
                _trainingCompletedMessageVisibility = value;
                OnPropertyChanged(nameof(TrainingCompletedMessageVisibility));
            }
        }

        public bool StartTrainingCanExecute
        {
            get { return string.IsNullOrEmpty(Error); }
        }

        public bool NavigateToNextPageCanExecute
        {
            get { return TrainingCompletedMessageVisibility == Visibility.Visible; }
        }

        public void Populate(TrainingDataDto trainingDataDto)
        {
            _trainingDataDto = trainingDataDto;
        }

        #region DataErrorInfo Implementation

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
                var hiddenLayersRegex = new Regex("^(\\d+)(;\\d+)*$");

                switch (columnName)
                {
                    case nameof(LearningRate):
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

                            break;
                        }
                    case nameof(NoEpochs):
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
                            break;
                        }
                    case nameof(MinError):
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
                            break;
                        }
                    case nameof(NeuralNetworkName):
                        {
                            if (string.IsNullOrEmpty(_neuralNetworkName))
                            {
                                error = string.Format(Properties.Resources.FieldIsRequiredValidationMessage,
                                    Properties.Resources.NeuralNetworkNameCaption);
                            }
                            break;
                        }
                    case nameof(NeuralNetworkSavePath):
                        {
                            if (!Directory.Exists(_neuralNetworkSavePath))
                            {
                                error = Properties.Resources.PathNoExistValidationMessage;
                            }

                            if (string.IsNullOrEmpty(_neuralNetworkSavePath))
                            {
                                error = string.Format(Properties.Resources.FieldIsRequiredValidationMessage,
                                    Properties.Resources.LocationToSaveCaption);
                            }
                            break;
                        }

                    case nameof(HiddenLayers):
                        {
                            if (string.IsNullOrEmpty(_hiddenLayers))
                            {
                                error = string.Format(Properties.Resources.FieldIsRequiredValidationMessage,
                                    Properties.Resources.HiddenLayersCaption);
                            }

                            if (!hiddenLayersRegex.IsMatch(_hiddenLayers))
                            {
                                error = Properties.Resources.InvalidHiddenLayersFormatValidationMessage;
                            }
                            break;
                        }
                }

                if (error != null)
                {
                    if (!_errorList.ContainsKey(columnName))
                    {
                        _errorList.Add(columnName, error);
                    }
                    else
                    {
                        _errorList[columnName] = error;
                    }
                }

                if (error == null && _errorList.ContainsKey(columnName))
                    _errorList.Remove(columnName);

                OnPropertyChanged(nameof(Error));
                OnPropertyChanged(nameof(StartTrainingCanExecute));

                return error;
            }
        }

        #endregion

        private void StartTraining()
        {
            var hiddenLayers = HiddenLayers.Split(';')
                .Select(x => Convert.ToInt32(x))
                .ToList();

            var annSevice = new AnnTrainingService(Convert.ToDouble(_minError),
                Convert.ToDouble(_learningRate),
                Convert.ToInt32(_noEpochs),
                Path.Combine(NeuralNetworkSavePath, $"{NeuralNetworkName}.txt"),
                _trainingDataDto.FilePath,
                _trainingDataDto.Sheet,
                _trainingDataDto.IdColumn,
                _trainingDataDto.InputVariables,
                _trainingDataDto.OutputVariables,
                hiddenLayers,
                _selectedActivationFunction,
                TrainingProgressChanged);

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

            // If completed
            TrainingCompletedMessageVisibility =
                e.ProgressPercentage == 100
                    ? Visibility.Visible
                    : Visibility.Hidden;
        }

        private string _minError;
        private string _learningRate;
        private string _noEpochs;
        private int _progressBarValue;
        private string _neuralNetworkName;
        private string _neuralNetworkSavePath;
        private string _hiddenLayers;
        private IDictionary<string, string> _errorList;
        private ActivationFunction _selectedActivationFunction;
        private TrainingDataDto _trainingDataDto;
        private Visibility _trainingCompletedMessageVisibility;
        private ICommand _startTraining;
        private ICommand _selectNeuralNetworkSavePath;
    }
}

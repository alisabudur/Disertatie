using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using SoftwareQualityPrediction.Models;
using SoftwareQualityPrediction.Services;
using SoftwareQualityPrediction.Utils;

namespace SoftwareQualityPrediction.ViewModels
{
    public class CrossValidationViewModel: BaseViewModel, IDataErrorInfo
    {
        public CrossValidationViewModel()
        {
            _k = "2";
            _errorList = new Dictionary<string, string>();
            _selectOutputColumnsViewModel = new SelectItemsViewModel();
            CrossValidationCompletedMessageVisibility = Visibility.Hidden;
        }

        public ICommand StartCrossValidationCommand
            => _startCrossValidation ?? (_startCrossValidation = new CommandHandler(StartCrossValidation, StartCrossValidationCanExecute));

        public string K
        {
            get { return _k; }
            set
            {
                _k = value;
                OnPropertyChanged(nameof(K));
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

        public SelectItemsViewModel SelectOutputColumnsViewModel
        {
            get { return _selectOutputColumnsViewModel; }
            set
            {
                _selectOutputColumnsViewModel = value;
                OnPropertyChanged(nameof(SelectOutputColumnsViewModel));
            }
        }

        public Visibility CrossValidationCompletedMessageVisibility
        {
            get { return _crossValidationCompletedMessageVisibility; }
            set
            {
                _crossValidationCompletedMessageVisibility = value;
                OnPropertyChanged(nameof(CrossValidationCompletedMessageVisibility));
            }
        }

        public void Populate(TrainingModel trainingModel)
        {
            _trainingModel = trainingModel;
            SelectOutputColumnsViewModel.UnselectedItems = new ObservableCollection<string>(trainingModel.TrainingData.AllColumns);
        }

        public CrossValidationModel Prepare()
        {
            return new CrossValidationModel
            {
                TrainingModel = _trainingModel,
                TestingModel = new TestingModel
                {
                    DataFilePath = _trainingModel.TrainingData.FilePath,
                    Sheet = _trainingModel.TrainingData.Sheet,
                    IdColumn = _trainingModel.TrainingData.IdColumn,
                    NeuralNetworkPath = _trainingModel.NeuralNetworkPath,
                    OutputVariables = SelectOutputColumnsViewModel.SelectedItems.ToList()
                },
                K = Convert.ToInt32(_k)
            };
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
                var intRegex = new Regex("^[1-9][0-9]*$");

                switch (columnName)
                {
                    case nameof(K):
                        {
                            if (string.IsNullOrEmpty(_k))
                            {
                                error = string.Format(Properties.Resources.FieldIsRequiredValidationMessage,
                                    Properties.Resources.KParameterCaption);
                            }

                            if (!intRegex.IsMatch(_k))
                            {
                                error = Properties.Resources.KValidationMessage;
                            }
                            break;
                        }

                    case nameof(SelectOutputColumnsViewModel):
                    {
                        if (SelectOutputColumnsViewModel.UnselectedItems.Any() &&
                            !SelectOutputColumnsViewModel.SelectedItems.Any())
                        {
                            error = string.Format(Properties.Resources.NoItemSelectedValidationMessage,
                                Properties.Resources.OutputColumnsCaption);
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
                OnPropertyChanged(nameof(StartCrossValidationCanExecute));

                return error;
            }
        }

        #endregion

        private void StartCrossValidation()
        {
            CrossValidationCompletedMessageVisibility = Visibility.Hidden;
            var crossValidationModel = Prepare();
            var crossValidationService = new CrossValidationService(crossValidationModel, CrossValidationProgressChanged);
            crossValidationService.Start();
        }

        private bool StartCrossValidationCanExecute()
        {
            return string.IsNullOrEmpty(Error);
        }

        private void CrossValidationProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBarValue = e.ProgressPercentage;

            // If completed
            CrossValidationCompletedMessageVisibility =
                e.ProgressPercentage == 100
                    ? Visibility.Visible
                    : Visibility.Hidden;
        }

        private string _k;
        private SelectItemsViewModel _selectOutputColumnsViewModel;
        private IDictionary<string, string> _errorList;
        private Visibility _crossValidationCompletedMessageVisibility;
        private int _progressBarValue;
        private TrainingModel _trainingModel;
        private ICommand _startCrossValidation;
    }
}

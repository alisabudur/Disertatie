using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Accord.Neuro;
using Microsoft.Win32;
using SoftwareQualityPrediction.Utils;

namespace SoftwareQualityPrediction.ViewModels
{
    public class TestingViewModel: BaseViewModel, IMediator, IDataErrorInfo
    {
        public TestingViewModel()
        {
            _uploadFileViewModel = new UploadFileViewModel {Mediator = this};
            _selectInputVariablesViewModel = new SelectItemsViewModel {Mediator = this};
            _selectOutputVariablesViewModel = new SelectItemsViewModel {Mediator = this};
            _errorList = new Dictionary<string, string>();
            _neuralNetworkInputsCount = 1;
            _testingCompletedMessageVisibility = Visibility.Hidden;
        }

        public ICommand SelectNeuralNetworkPathCommand
            => _selectNeuralNetworkPath ?? (_selectNeuralNetworkPath = new CommandHandler(SelectNeuralNetworkPath,
                   () => true));

        public ICommand StartTestingCommand
            => _selectNeuralNetworkPath ?? (_selectNeuralNetworkPath = new CommandHandler(StartTesting,
                   () => StartTestingCanExecute));

        public UploadFileViewModel UploadFileViewModel
        {
            get { return _uploadFileViewModel; }
            set
            {
                _uploadFileViewModel = value;
                OnPropertyChanged(nameof(UploadFileViewModel));
            }
        }

        public SelectItemsViewModel SelectInputVariablesViewModel
        {
            get { return _selectInputVariablesViewModel; }
            set
            {
                _selectInputVariablesViewModel = value;
                OnPropertyChanged(nameof(SelectInputVariablesViewModel));
            }
        }

        public SelectItemsViewModel SelectOutputVariablesViewModel
        {
            get { return _selectOutputVariablesViewModel; }
            set
            {
                _selectOutputVariablesViewModel = value;
                OnPropertyChanged(nameof(SelectOutputVariablesViewModel));
            }
        }

        public string NeuralNetworkPath
        {
            get { return _neuralNetworkPath; }
            set
            {
                _neuralNetworkPath = value;
                OnPropertyChanged(nameof(NeuralNetworkPath));
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

        public Visibility TestingCompletedMessageVisibility
        {
            get { return _testingCompletedMessageVisibility; }
            set
            {
                _testingCompletedMessageVisibility = value;
                OnPropertyChanged(nameof(TestingCompletedMessageVisibility));
            }
        }

        public bool StartTestingCanExecute
        {
            get
            {
                return !string.IsNullOrEmpty(UploadFileViewModel.FilePath)
                       && !string.IsNullOrEmpty(UploadFileViewModel.SelectedSheet)
                       && !string.IsNullOrEmpty(UploadFileViewModel.SelectedIdColumn)
                       && string.IsNullOrEmpty(Error);
            }
        }

        #region Mediator Implementation

        public void Send(object message, IColleague colleague)
        {
            if (colleague == _uploadFileViewModel)
            {
                _selectInputVariablesViewModel.Receive(message);
                _selectOutputVariablesViewModel.Receive(message);
            }

            OnPropertyChanged(nameof(SelectInputVariablesViewModel));
            OnPropertyChanged(nameof(SelectOutputVariablesViewModel));
        }

        #endregion

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

                switch (columnName)
                {
                    case nameof(SelectInputVariablesViewModel):
                        {
                            if (SelectInputVariablesViewModel.SelectedItems.Count != _neuralNetworkInputsCount)
                            {
                                error = string.Format(Properties.Resources.ShouldBeExactlyValidationMessage,
                                    _neuralNetworkInputsCount);
                            }

                            if (SelectInputVariablesViewModel.UnselectedItems.Any() &&
                                !SelectInputVariablesViewModel.SelectedItems.Any())
                            {
                                error = string.Format(Properties.Resources.NoItemSelectedValidationMessage,
                                    Properties.Resources.InputVariablesCaption);
                            }

                            break;
                        }
                    case nameof(SelectOutputVariablesViewModel):
                    {
                        if (SelectOutputVariablesViewModel.UnselectedItems.Any() &&
                            !SelectOutputVariablesViewModel.SelectedItems.Any())
                        {
                            error = string.Format(Properties.Resources.NoItemSelectedValidationMessage,
                                Properties.Resources.OutputVariablesCaption);
                        }

                        break;
                    }
                    case nameof(NeuralNetworkPath):
                    {
                        if (!File.Exists(_neuralNetworkPath))
                        {
                            error = Properties.Resources.PathNoExistValidationMessage;
                        }

                        if (string.IsNullOrEmpty(_neuralNetworkPath))
                        {
                            error = string.Format(Properties.Resources.FieldIsRequiredValidationMessage,
                                Properties.Resources.NeuralNetworkPathCaption);
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
                OnPropertyChanged(nameof(StartTestingCanExecute));

                return error;
            }
        }

        #endregion

        private void SelectNeuralNetworkPath()
        {
            var op = new OpenFileDialog();
            op.Title = "Select a file";
            op.Filter = "Text|*.txt";

            if (op.ShowDialog() == true)
            {
                NeuralNetworkPath = op.FileName;

                // If is valid
                if (string.IsNullOrEmpty(this[nameof(NeuralNetworkPath)]))
                {
                    _neuralNetworkInputsCount = Network.Load(NeuralNetworkPath).InputsCount;
                }
            }
        }

        private void StartTesting()
        {

        }

        private UploadFileViewModel _uploadFileViewModel;
        private SelectItemsViewModel _selectInputVariablesViewModel;
        private SelectItemsViewModel _selectOutputVariablesViewModel;
        private string _neuralNetworkPath;
        private int _neuralNetworkInputsCount;
        private int _progressBarValue;
        private Visibility _testingCompletedMessageVisibility;
        private IDictionary<string, string> _errorList;
        private ICommand _selectNeuralNetworkPath;
        private ICommand _startTesting;
    }
}

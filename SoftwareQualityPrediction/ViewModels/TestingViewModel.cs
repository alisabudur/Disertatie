using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using SoftwareQualityPrediction.Services;
using SoftwareQualityPrediction.Utils;

namespace SoftwareQualityPrediction.ViewModels
{
    public class TestingViewModel: BaseViewModel, IMediator, IDataErrorInfo
    {
        public TestingViewModel()
        {
            _uploadFileViewModel = new UploadFileViewModel {Mediator = this};
            _selectOutputColumnsViewModel = new SelectItemsViewModel {Mediator = this};
            _errorList = new Dictionary<string, string>();
            _testingCompletedMessageVisibility = Visibility.Hidden;
        }

        public ICommand SelectNeuralNetworkPathCommand
            => _selectNeuralNetworkPath ?? (_selectNeuralNetworkPath = new CommandHandler(SelectNeuralNetworkPath,
                   () => true));

        public ICommand StartTestingCommand
            => _startTesting ?? (_startTesting = new CommandHandler(StartTesting, StartTestingCanExecute));

        public UploadFileViewModel UploadFileViewModel
        {
            get { return _uploadFileViewModel; }
            set
            {
                _uploadFileViewModel = value;
                OnPropertyChanged(nameof(UploadFileViewModel));
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

        #region Mediator Implementation

        public void Send(object message, IColleague colleague)
        {
            if (colleague == _uploadFileViewModel)
            {
                _selectOutputColumnsViewModel.Receive(message);
                OnPropertyChanged(nameof(UploadFileViewModel));
                OnPropertyChanged(nameof(NeuralNetworkPath));
                ProgressBarValue = 0;
                TestingCompletedMessageVisibility = Visibility.Hidden;
            }

            OnPropertyChanged(nameof(SelectOutputColumnsViewModel));
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
                    case nameof(NeuralNetworkPath):
                    {
                        if (string.IsNullOrEmpty(_neuralNetworkPath))
                        {
                            error = string.Format(Properties.Resources.FieldIsRequiredValidationMessage,
                                Properties.Resources.NeuralNetworkPathCaption);
                            break;
                        }

                        if (!File.Exists(_neuralNetworkPath))
                        {
                            error = Properties.Resources.PathNoExistValidationMessage;
                            break;
                        }

                        var nnColumns = AnnTestingService.GetNeuralNetworkInputVariables(NeuralNetworkPath);

                        if (!nnColumns.All(x => UploadFileViewModel.Columns.Contains(x)))
                        {
                            error = Properties.Resources.NotAllNNVariablesAreMappedValidationMessage;
                        }

                        break;
                    }

                    case nameof(UploadFileViewModel):
                    {
                        if (string.IsNullOrEmpty(UploadFileViewModel.SelectedSheet))
                        {
                            error = string.Format(Properties.Resources.FieldIsRequiredValidationMessage,
                                Properties.Resources.SheetCaption);
                        }

                        if (string.IsNullOrEmpty(UploadFileViewModel.SelectedIdColumn))
                        {
                            error = string.Format(Properties.Resources.FieldIsRequiredValidationMessage,
                                Properties.Resources.IdColumnCaption);
                        }

                        if (string.IsNullOrEmpty(UploadFileViewModel.FilePath))
                        {
                            error = string.Format(Properties.Resources.FieldIsRequiredValidationMessage,
                                Properties.Resources.FileFieldCaption);
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
                ((CommandHandler)StartTestingCommand).RaiseCanExecuteChanged();
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
                NeuralNetworkPath = op.FileName.Replace(@"\\", @"\");
            }
        }

        private void StartTesting()
        {
            var annService = new AnnTestingService(_neuralNetworkPath,
                UploadFileViewModel.FilePath,
                UploadFileViewModel.SelectedSheet,
                UploadFileViewModel.SelectedIdColumn,
                SelectOutputColumnsViewModel.SelectedItems.ToList(),
                TestingProgressChanged);

            annService.StartTesting();
        }

        private bool StartTestingCanExecute()
        {
            return string.IsNullOrEmpty(Error);
        }

        private void TestingProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBarValue = e.ProgressPercentage;

            // If completed
            TestingCompletedMessageVisibility =
                e.ProgressPercentage == 100
                    ? Visibility.Visible
                    : Visibility.Hidden;
        }

        private UploadFileViewModel _uploadFileViewModel;
        private SelectItemsViewModel _selectOutputColumnsViewModel;
        private string _neuralNetworkPath;
        private int _progressBarValue;
        private Visibility _testingCompletedMessageVisibility;
        private IDictionary<string, string> _errorList;
        private ICommand _selectNeuralNetworkPath;
        private ICommand _startTesting;
    }
}

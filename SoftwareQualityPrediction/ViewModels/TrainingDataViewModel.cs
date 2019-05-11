using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SoftwareQualityPrediction.Dtos;
using SoftwareQualityPrediction.Utils;

namespace SoftwareQualityPrediction.ViewModels
{
    public class TrainingDataViewModel: BaseViewModel, IMediator, IDataErrorInfo
    {
        public TrainingDataViewModel()
        {
            _uploadFileViewModel = new UploadFileViewModel {Mediator = this};
            _selectInputVariablesViewModel = new SelectItemsViewModel {Mediator = this};
            _selectOutputVariablesViewModel = new SelectItemsViewModel {Mediator = this};
            _errorList = new Dictionary<string, string>();
        }

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

        public bool NavigateToNextPageCanExecute
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
                OnPropertyChanged(nameof(NavigateToNextPageCanExecute));

                return error;
            }
        }

        #endregion

        public TrainingDataDto PrepareDto()
        {
            return new TrainingDataDto
            {
                FilePath = UploadFileViewModel.FilePath,
                Sheet = UploadFileViewModel.SelectedSheet,
                IdColumn = UploadFileViewModel.SelectedIdColumn,
                InputVariables = SelectInputVariablesViewModel.SelectedItems.ToList(),
                OutputVariables = SelectOutputVariablesViewModel.SelectedItems.ToList()
            };
        }

        private UploadFileViewModel _uploadFileViewModel;
        private SelectItemsViewModel _selectInputVariablesViewModel;
        private SelectItemsViewModel _selectOutputVariablesViewModel;
        private IDictionary<string, string> _errorList;
    }
}

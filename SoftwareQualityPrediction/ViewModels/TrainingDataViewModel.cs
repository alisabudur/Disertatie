using SoftwareQualityPrediction.Utils;

namespace SoftwareQualityPrediction.ViewModels
{
    public class TrainingDataViewModel: BaseViewModel, IMediator
    {
        public TrainingDataViewModel()
        {
            _uploadFileViewModel = new UploadFileViewModel();
            _uploadFileViewModel.Mediator = this;
            _selectInputVariablesViewModel = new SelectItemsViewModel();
            _selectInputVariablesViewModel.Mediator = this;
            _selectOutputVariablesViewModel = new SelectItemsViewModel();
            _selectOutputVariablesViewModel.Mediator = this;
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

        public void Send(object message, IColleague colleague)
        {
            if (colleague == _uploadFileViewModel)
            {
                _selectInputVariablesViewModel.Receive(message);
                _selectOutputVariablesViewModel.Receive(message);
            }
        }

        private UploadFileViewModel _uploadFileViewModel;
        private SelectItemsViewModel _selectInputVariablesViewModel;
        private SelectItemsViewModel _selectOutputVariablesViewModel;
    }
}

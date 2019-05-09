namespace SoftwareQualityPrediction.ViewModels
{
    public class TrainingViewModel: BaseViewModel
    {
        public TrainingViewModel()
        {
            _uploadFileViewModel = new UploadFileViewModel();
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

        private UploadFileViewModel _uploadFileViewModel;
    }
}

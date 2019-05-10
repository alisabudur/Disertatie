using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using SoftwareQualityPrediction.Utils;
using SoftwareQualityPrediction.Views;

namespace SoftwareQualityPrediction.ViewModels
{
    public class MainWindowViewModel: BaseViewModel
    {
        public MainWindowViewModel()
        {
            _canExecute = true;

            _pages = new Dictionary<string, Page>()
            {
                { Properties.Resources.TrainingDataCaption, new TrainingDataPage()},
            };

            Page = _pages[Properties.Resources.TrainingDataCaption];
        }

        public ICommand NavigateToTrainingDataPageCommand => _navigateToTrainingDataPage ?? (_navigateToTrainingDataPage = new CommandHandler(
                                                      () => { Page = _pages[Properties.Resources.TrainingDataCaption]; }, _canExecute));


        public Page Page

        {
            get { return _currentPage; }
            set { _currentPage = value; OnPropertyChanged("Page"); }
        }

        private bool _canExecute;
        private Page _currentPage;
        private IDictionary<string, Page> _pages;
        private ICommand _navigateToTrainingDataPage;
    }
}

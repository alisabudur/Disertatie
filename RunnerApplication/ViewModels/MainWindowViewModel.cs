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
            _pages = new Dictionary<string, Page>()
            {
                { Properties.Resources.TrainingDataCaption, new TrainingDataPage()},
                { Properties.Resources.TestingCaption, new TestingPage()},
            };

            Page = _pages[Properties.Resources.TrainingDataCaption];
        }

        public ICommand NavigateToTrainingDataPageCommand => _navigateToTrainingDataPage ?? (_navigateToTrainingDataPage = new CommandHandler(
                                                      () => { Page = _pages[Properties.Resources.TrainingDataCaption]; },
                                                      () => true));

        public ICommand NavigateToTestingPageCommand => _navigateToTestingPage ?? (_navigateToTestingPage = new CommandHandler(
                                                                 () => { Page = _pages[Properties.Resources.TestingCaption]; },
                                                                 () => true));


        public Page Page

        {
            get { return _currentPage; }
            set { _currentPage = value; OnPropertyChanged("Page"); }
        }

        private Page _currentPage;
        private IDictionary<string, Page> _pages;
        private ICommand _navigateToTrainingDataPage;
        private ICommand _navigateToTestingPage;
    }
}

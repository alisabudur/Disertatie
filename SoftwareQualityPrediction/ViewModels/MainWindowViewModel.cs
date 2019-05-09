using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using SoftwareQualityPrediction.Utils;
using SoftwareQualityPrediction.Views;

namespace SoftwareQualityPrediction.ViewModels
{
    public class MainWindowViewModel: BaseViewModel
    {
        private bool _canExecute;
        private Page _currentPage;
        private IDictionary<string, Page> _pages;
        private ICommand _navigateToTrainingView;


        public MainWindowViewModel()
        {
            _canExecute = true;

            _pages = new Dictionary<string, Page>()
            {
                { Properties.Resources.TrainingCaption, new TrainingPage()},
            };

            Page = _pages[Properties.Resources.TrainingCaption];
        }

        public Page Page

        {
            get { return _currentPage; }
            set { _currentPage = value; OnPropertyChanged("Page"); }
        }

        public ICommand NavigateToTrainingView => _navigateToTrainingView ?? (_navigateToTrainingView = new CommandHandler(
                                                 () => { Page = _pages[Properties.Resources.TrainingCaption]; }, _canExecute));

    }
}

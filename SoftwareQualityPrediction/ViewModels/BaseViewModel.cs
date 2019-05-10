using System.ComponentModel;
using System.Windows.Controls;

namespace SoftwareQualityPrediction.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public Page Page

        {
            get { return _currentPage; }
            set
            {
                _currentPage = value;
                OnPropertyChanged("Page");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyname)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyname));
        }

        private Page _currentPage;
    }
}

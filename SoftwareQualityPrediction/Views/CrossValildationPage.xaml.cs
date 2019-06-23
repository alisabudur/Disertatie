using System.Windows.Controls;
using SoftwareQualityPrediction.Models;
using SoftwareQualityPrediction.ViewModels;

namespace SoftwareQualityPrediction.Views
{
    /// <summary>
    /// Interaction logic for CrossValildationPage.xaml
    /// </summary>
    public partial class CrossValildationPage : Page
    {
        public CrossValildationPage(TrainingModel trainingModel)
        {
            InitializeComponent();
            var viewModel = (CrossValidationViewModel) DataContext;
            viewModel.Populate(trainingModel);
        }
    }
}

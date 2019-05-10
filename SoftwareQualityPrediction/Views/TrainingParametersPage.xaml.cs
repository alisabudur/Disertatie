using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using SoftwareQualityPrediction.Dtos;
using SoftwareQualityPrediction.ViewModels;

namespace SoftwareQualityPrediction.Views
{
    /// <summary>
    /// Interaction logic for TrainingParametersPage.xaml
    /// </summary>
    public partial class TrainingParametersPage : Page
    {
        public TrainingParametersPage(TrainingDataDto trainingDataDto)
        {
            InitializeComponent();
            var viewModel = (TrainingParametersViewModel) DataContext;
            viewModel.Populate(trainingDataDto);
        }
    }
}

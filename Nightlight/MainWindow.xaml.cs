using System.Windows;
using BuisnessLogic;

namespace Nightlight
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            NightlightModel model = new NightlightModel();
            MainViewModel viewModel = new MainViewModel(model);

            DataContext = viewModel;
        }
    }
}
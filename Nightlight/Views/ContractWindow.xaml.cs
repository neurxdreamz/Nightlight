using System.Windows;

namespace Nightlight.Views
{
    public partial class ContractWindow : Window
    {
        public ContractWindow(ContractViewModel contract)
        {
            InitializeComponent();
            DataContext = contract;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
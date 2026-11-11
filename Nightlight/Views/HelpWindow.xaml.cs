using System.Windows;

namespace Nightlight.Views
{
    public partial class HelpWindow : Window
    {
        public string TitleText { get; set; }
        public string HelpText { get; set; }

        public HelpWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
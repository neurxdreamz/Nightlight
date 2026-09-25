using System.Windows;
using System.Windows.Controls;

namespace Nightlight.Views
{
    public partial class SleepTimerView : UserControl
    {
        public SleepTimerView()
        {
            InitializeComponent();
            DataContextChanged += SleepTimerView_DataContextChanged;
        }

        private void SleepTimerView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SleepTimerViewModel oldVm = e.OldValue as SleepTimerViewModel;
            if (oldVm != null)
            {
                oldVm.ContractRequested -= ShowContractWindow;
            }

            SleepTimerViewModel newVm = e.NewValue as SleepTimerViewModel;
            if (newVm != null)
            {
                newVm.ContractRequested += ShowContractWindow;
            }
        }

        private void ShowContractWindow(ContractViewModel contract)
        {
            ContractWindow window = new ContractWindow(contract);
            window.Owner = Window.GetWindow(this);
            window.ShowDialog();
        }
    }
}
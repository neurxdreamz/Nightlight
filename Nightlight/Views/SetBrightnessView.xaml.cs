using System.Windows;
using System.Windows.Controls;

namespace Nightlight.Views
{
    public partial class SetBrightnessView : UserControl
    {
        public SetBrightnessView()
        {
            InitializeComponent();
            DataContextChanged += SetBrightnessView_DataContextChanged;
        }

        private void SetBrightnessView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetBrightnessViewModel oldVm = e.OldValue as SetBrightnessViewModel;
            if (oldVm != null)
            {
                oldVm.ContractRequested -= ShowContractWindow;
            }

            SetBrightnessViewModel newVm = e.NewValue as SetBrightnessViewModel;
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
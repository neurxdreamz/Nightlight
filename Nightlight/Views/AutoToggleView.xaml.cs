using System.Windows;
using System.Windows.Controls;

namespace Nightlight.Views
{
    public partial class AutoToggleView : UserControl
    {
        public AutoToggleView()
        {
            InitializeComponent();
            DataContextChanged += AutoToggleView_DataContextChanged;
        }

        private void AutoToggleView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            AutoToggleViewModel oldVm = e.OldValue as AutoToggleViewModel;
            if (oldVm != null)
            {
                oldVm.ContractRequested -= ShowContractWindow;
            }

            AutoToggleViewModel newVm = e.NewValue as AutoToggleViewModel;
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